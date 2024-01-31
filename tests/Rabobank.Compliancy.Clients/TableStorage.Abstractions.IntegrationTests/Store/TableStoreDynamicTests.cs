using System;
using TableStorage.Abstractions.Store;

namespace TableStorage.Abstractions.IntegrationTests.Store
{
    public partial class TableStoreDynamicTests : TestBase, IDisposable
    {
        private readonly ITableStoreDynamic _tableStorageDynamic;
        private const string TableName = "TestTableDynamic";

        public TableStoreDynamicTests()
        {
            _tableStorageDynamic = new TableStoreDynamic(TableName, ConnectionString);
        }

        public void Dispose()
        {
            _tableStorageDynamic.DeleteTable();
        }
    }
}