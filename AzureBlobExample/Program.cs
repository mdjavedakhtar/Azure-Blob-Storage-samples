using AzureBlobExample;

namespace AzureBlobExample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;

    public class GlobalVariables
    {
        public static string storageConnectionString { get; set; } = "DefaultEndpointsProtocol=https;AccountName=blobteststorageacct;AccountKey===;EndpointSuffix=core.windows.net";
        public static BlobContainerClient? containerClient { get; set; }// Nullable
        public static string? localFilePath { get; set; }
        public static string? fileName { get; set; }
        public static BlobClient? blobClient { get; set; }

        public static List<BlobContainerItem>? containers { get; set; } = new List<BlobContainerItem>();
    }

    public class Program
    {
        private static void Main()
        {
            // Make the local File Directory
            string localPath = "./data/";

            Directory.CreateDirectory(localPath);

            GlobalVariables.fileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
            GlobalVariables.localFilePath = Path.Combine(localPath, GlobalVariables.fileName);

            ListContainers().GetAwaiter().GetResult();
            
            // Run the examples asynchronously, wait for the results before proceeding
            //CreateNewContainer().GetAwaiter().GetResult();

            GlobalVariables.blobClient = GlobalVariables.containerClient.GetBlobClient(GlobalVariables.fileName);//get blobclient after container is created

            UploadtoBlob().GetAwaiter().GetResult();
            ListBlobsInStorage().GetAwaiter().GetResult();

            DownloadBlobinContainer().GetAwaiter().GetResult();

            DeleteContainer().GetAwaiter().GetResult();
            
            Console.WriteLine("Press enter to exit the sample application.");
            Console.ReadLine();

            static async Task ListContainers()
            {
                
                BlobServiceClient blobServiceClient = new BlobServiceClient(GlobalVariables.storageConnectionString);
                // List Containers in the container
                Console.WriteLine("Listing Containers...");
                await foreach (BlobContainerItem containerItem in blobServiceClient.GetBlobContainersAsync())
                {
                    GlobalVariables.containers.Add(containerItem);
                    Console.WriteLine("\t" + containerItem.Name);
                }

                Console.WriteLine("\nType name to select one or NEW to create a new container");
                Console.WriteLine("Press 'Enter' to continue.");
                string commandReceived=Console.ReadLine();
                if(commandReceived=="NEW")
                {
                    Console.WriteLine("\nCreate new container command");
                    Console.WriteLine("\nEnter Container Name:");
                    commandReceived = Console.ReadLine();
                    await CreateNewContainer(commandReceived);
                }
                else
                {
                    //string containerItem = GlobalVariables.containers.Find(x => x.Name == commandReceived).Name;
                    GlobalVariables.containerClient = blobServiceClient.GetBlobContainerClient(commandReceived);

                    //Console.WriteLine();
                }
            }

            static async Task CreateNewContainer(string containerName)
            {
                // Create a client that can authenticate with a connection string
                BlobServiceClient blobServiceClient = new BlobServiceClient(GlobalVariables.storageConnectionString);

                //Create a unique name for the container
                //string containerName = "wtblob" + Guid.NewGuid().ToString();

                // Create the container and return a container client object
                GlobalVariables.containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

                Console.WriteLine("A container named '" + containerName + "' has been created. ");
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
            }


            static async Task UploadtoBlob()
            {
                // Create a local file in the ./data/ directory for uploading and downloading

                // Write text to the file
                await File.WriteAllTextAsync(GlobalVariables.localFilePath, "Hello, World!");
                if (GlobalVariables.containerClient != null)
                {
                    // Get a reference to the blob
                    

                    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", GlobalVariables.blobClient.Uri);

                    // Open the file and upload its data
                    using (FileStream uploadFileStream = File.OpenRead(GlobalVariables.localFilePath))
                    {
                        await GlobalVariables.blobClient.UploadAsync(uploadFileStream);
                        uploadFileStream.Close();
                    }

                    Console.WriteLine("\nThe file was uploaded." );
                    Console.WriteLine("Press 'Enter' to continue.");
                    Console.ReadLine();
                } else
                {
                    Console.WriteLine("Container is null");
                }
            }

            static async Task ListBlobsInStorage()
            {
                // List blobs in the container
                Console.WriteLine("Listing blobs...");
                await foreach (BlobItem blobItem in GlobalVariables.containerClient.GetBlobsAsync())
                {
                    Console.WriteLine("\t" + blobItem.Name);
                }

                Console.WriteLine("\nYou can also verify by looking inside the container in the portal.");
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
            }

            static async Task DownloadBlobinContainer()
            {
                // Download the blob to a local file
                // Append the string "DOWNLOADED" before the .txt extension 
                string downloadFilePath = GlobalVariables.localFilePath.Replace(".txt", "DOWNLOADED.txt");

                Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

                // Download the blob's contents and save it to a file
                BlobDownloadInfo download = await GlobalVariables.blobClient.DownloadAsync();

                using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
                {
                    await download.Content.CopyToAsync(downloadFileStream);
                }
                Console.WriteLine("\nFile downloaded.");
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
            }

            static async Task DeleteContainer()
            {
                // Delete the container and clean up local files created
                Console.WriteLine("\n\nDeleting blob container...");
                await GlobalVariables.containerClient.DeleteAsync();

                //Console.WriteLine("Deleting the local source and downloaded files...");
                //File.Delete(localFilePath);
                //File.Delete(downloadFilePath);

                Console.WriteLine("Finished cleaning up.");
            }

        }
    }
}