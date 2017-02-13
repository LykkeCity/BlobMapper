

namespace MvcApp
{


    public static class ServiceLocator
    {
        public static AzureStorage.Blob.AzureBlobStorage BlobStorage;

        public static string BlobContainerName { get; set; }
    }

}