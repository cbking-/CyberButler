namespace CyberButler.DatabaseRecords
{
    abstract class BaseRecord
    {
        protected DatabaseConnection db = new DatabaseConnection();

        public abstract void Insert();
    }
}
