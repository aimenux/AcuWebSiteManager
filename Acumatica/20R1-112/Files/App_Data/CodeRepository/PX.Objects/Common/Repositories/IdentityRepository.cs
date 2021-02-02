using PX.Data;

namespace PX.Objects.Common.Repositories
{
	/// <summary>
	/// Repository providing basic methods for tables that have a unique ID field.
	/// </summary>
	public class IdentityRepository<TNode, TIdentityField> : RepositoryBase<TNode>
		where TNode : class, IBqlTable, new()
		where TIdentityField : IBqlField
	{
		public IdentityRepository(PXGraph graph) : base(graph)
		{ }

		public TNode FindByID(object id) => SelectSingle<Where<TIdentityField, Equal<Required<TIdentityField>>>>(id);

		public TNode GetByID(object id) => ForceNotNull(FindByID(id));
	}
}
