using HtmlAgilityPack;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using System;
using System.Linq;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
	public abstract class EmbeddedImagesExtractorExtension<TGraph, TDocument, THtmlField> : AttachmentsHandlerExtension<TGraph>
		where TGraph : PXGraph, new()
		where TDocument : class, INotable, IBqlTable, new()
		where THtmlField : BqlString.Field<THtmlField>
	{
		protected virtual bool ShowHtmlFieldWarningForException => true;

		internal virtual bool ExtractionIsActive => Base.Caches<TDocument>().Current != null;


		[PXOverride]
		public virtual void Persist(Action persist)
		{
			if(!ExtractionIsActive)
			{
				persist();
				return;
			}

			Action revertCallback = null;
			FormatExceptionBehaviour? eh = null;
			ImageExtractor.Base64FormatException ex = null;
			try
			{
				ExtractEmbeddedImages(out revertCallback);
				ProcessWhenNoExceptionThrown();
			}
			catch (ImageExtractor.Base64FormatException b64e)
			{
				PXTrace.WriteError(b64e);

				if (revertCallback != null)
				{
					revertCallback();
					revertCallback = null;
				}

				eh = ProcessBase64FormatException(b64e);
				if (eh == FormatExceptionBehaviour.Throw)
					throw new PXSetPropertyException(b64e, PXErrorLevel.Error, Messages.CannotDecodeBase64ContentExplicit);
				ex = b64e;
			}

			try
			{
				persist();
			}
			catch
			{
				revertCallback?.Invoke();
				throw;
			}
			if (ex != null && eh == FormatExceptionBehaviour.ThrowWarningAfterPersist)
			{
				throw new PXSetPropertyException(ex, PXErrorLevel.Warning, Messages.CannotDecodeBase64Content);
			}
		}

		private protected virtual FormatExceptionBehaviour ProcessBase64FormatException(ImageExtractor.Base64FormatException e)
		{
			if (ShowHtmlFieldWarningForException)
			{
				var cache = Base.Caches<TDocument>();
				var entity = cache.Current;
				if (entity != null)
				{
					cache.RaiseExceptionHandling<THtmlField>(
						entity,
						cache.GetValue<THtmlField>(entity),
						new PXSetPropertyException<THtmlField>(Messages.CannotDecodeBase64Content, PXErrorLevel.RowWarning));

					return FormatExceptionBehaviour.ThrowWarningAfterPersist;
				}
			}

			return FormatExceptionBehaviour.Throw;
		}

		private protected virtual void ProcessWhenNoExceptionThrown() { }

		private void ExtractEmbeddedImages(out Action revertCallback)
		{
			revertCallback = null;
			Base.EnsureCachePersistence(typeof(TDocument));
			var documentCache = Base.Caches[typeof(TDocument)];

			object document = documentCache.Current;
			if (document == null)
				return;

			Guid? noteId = documentCache.GetValue(document, nameof(INotable.NoteID)) as Guid?;
			if (noteId == null)
				return;
			string html = documentCache.GetValue<THtmlField>(document) as string;
			if (String.IsNullOrEmpty(html))
				return;

			var extractor = new PX.Data.ImageExtractor();
			Action revertCallback_ = null;
			(string src, string title) getSrcAndTitle(PX.Data.ImageExtractor.ImageInfo img)
			{
				var (src, title, cb) = AddExtractedImage(noteId.Value, img);
				revertCallback_ += cb;
				return (src, title);
			};
			try
			{
				if (extractor.ExtractEmbedded(html, getSrcAndTitle, out var newHtml, out var _))
				{
					documentCache.SetValue<THtmlField>(document, newHtml);
				}
			}
			finally
			{
				revertCallback = revertCallback_ + (() => documentCache.SetValue<THtmlField>(documentCache.Current, html));
			}
		}

		private (string src, string title, Action revertCallback) AddExtractedImage(Guid noteId, PX.Data.ImageExtractor.ImageInfo img)
		{
			InsertFile(new FileDto(noteId, img.Name, img.Bytes, img.ID), out Action revertCallback);

			string url = ImageExtractor.PREFIX_IMG_BY_FILEID + img.CID;
			return (url, img.Name, revertCallback);
		}

		private protected enum FormatExceptionBehaviour
		{
			Throw,
			ThrowWarningAfterPersist,
			DontThrow,
		}

		public abstract class WithFieldForExceptionPersistence<TExceptionField> : EmbeddedImagesExtractorExtension<TGraph, TDocument, THtmlField>
			where TExceptionField : BqlString.Field<TExceptionField>
		{
			protected override bool ShowHtmlFieldWarningForException => false;

			[PXInternalUseOnly]
			public static class ExceptionHtmlConsts
			{
				public const string Tag = "div";
				public const string Style = "style=\"color:orange\"";
				public const string Base64FormatExceptionDataTagValue = "Base64ContentExtraction";
				public const string PXExceptionDataTag = "data-px-marker-exception";
				public const string HtmlExceptionTag = "<" + Tag + " " + Style + " " + PXExceptionDataTag + "=\"" + Base64FormatExceptionDataTagValue + "\">{0}</" + Tag + ">";
			}

			private protected override FormatExceptionBehaviour ProcessBase64FormatException(ImageExtractor.Base64FormatException e)
			{
				var cache = Base.Caches<TDocument>();
				var entity = cache.Current;
				if (entity == null)
					// shouldn't ever happen
					return base.ProcessBase64FormatException(e);

				var value = cache.GetValue<TExceptionField>(entity) as string;
				if(!value.IsNullOrEmpty())
				{
					try
					{
						var doc = new HtmlDocument();
						doc.LoadHtml(value);
						if (doc.DocumentNode
								.Descendants(ExceptionHtmlConsts.Tag)
								.Where(el => el.Attributes[ExceptionHtmlConsts.PXExceptionDataTag]?.Value == ExceptionHtmlConsts.Base64FormatExceptionDataTagValue)
								.Any())
							// do nothing - exception already exists
							return FormatExceptionBehaviour.DontThrow;

						value += "<br><br>"
							+ string.Format(ExceptionHtmlConsts.HtmlExceptionTag,
								PXMessages.LocalizeNoPrefix(Messages.CannotDecodeBase64ContentExplicit));
					}
					catch (Exception ex)
					{
						PXTrace.WriteError(ex);
					}
				}
				else
				{
					value = string.Format(ExceptionHtmlConsts.HtmlExceptionTag,
								PXMessages.LocalizeNoPrefix(Messages.CannotDecodeBase64ContentExplicit));
				}

				cache.SetValue<TExceptionField>(entity, value);

				return FormatExceptionBehaviour.DontThrow;
			}

			private protected override void ProcessWhenNoExceptionThrown()
			{
				var cache = Base.Caches<TDocument>();
				var entity = cache.Current;
				if (entity == null && cache.GetValue<TExceptionField>(entity) == null)
					return;

				var value = cache.GetValue<TExceptionField>(entity) as string;
				if (!value.IsNullOrEmpty())
				{
					try
					{
						var doc = new HtmlDocument();
						doc.LoadHtml(value);
						var nodes = doc.DocumentNode
								.Descendants(ExceptionHtmlConsts.Tag)
								.Where(el => el.Attributes[ExceptionHtmlConsts.PXExceptionDataTag]?.Value == ExceptionHtmlConsts.Base64FormatExceptionDataTagValue)
								.ToList();
						foreach (var node in nodes)
						{
							// remove empty lines
							if(node.PreviousSibling?.Name == "br")
							{
								doc.DocumentNode.RemoveChild(node.PreviousSibling);
								if (node.PreviousSibling?.PreviousSibling?.Name == "br")
								{
									doc.DocumentNode.RemoveChild(node.PreviousSibling.PreviousSibling);
								}
							}
							if (node.NextSibling?.Name == "br")
							{
								doc.DocumentNode.RemoveChild(node.NextSibling);
							}
							doc.DocumentNode.RemoveChild(node);
						}
						cache.SetValue<TExceptionField>(entity, doc.DocumentNode.OuterHtml);
					}
					catch (Exception ex)
					{
						PXTrace.WriteError(ex);
					}
				}
			}
		}
	}
}