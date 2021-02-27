namespace Lib.Helpers
{
    public interface IFileHelper
    {
        void ModifyFile(string filepath, string oldString, string newString);
    }
}
