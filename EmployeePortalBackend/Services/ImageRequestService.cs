using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.S3;
using Amazon.S3.Model;
using Azure;
using EmployeePortalBackend.DTO.ìmageDtos;
using EmployeePortalBackend.Enums;
using EmployeePortalBackend.Interface;
using EmployeePortalBackend.Model;
using System.Net;

namespace EmployeePortalBackend.Services
{
    public class ImageRequestService
    {
        IBasicCustomerRepository _customerRepository;
        IIdRequestRepository _repository;
        VaultService encryption;

        string accessKey = "minioAdmin";
        string secretKey = "EFNJOjklIE-FOHkk767FE90238k902-UEF2EFN3FIO-EHD";
        string minioEndpoint = "http://minio:9000";
        string bucketName = "helpdesk-customer-uploads";

        string krakendEndpoint = "http://localhost:8082/idrequest";

        AmazonS3Config s3Config;
        public ImageRequestService(IBasicCustomerRepository repo, IIdRequestRepository trepo, VaultService vc)
        {
            s3Config = new AmazonS3Config
            {
                ServiceURL = "http://minio:9000",
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1" // Add this line!
            };

            _customerRepository = repo;
            _repository = trepo;
            encryption = vc;
        }
        //generate the inital upload url

        public async Task<string?> GenerateInitialUploadUrl(CreateUploadRequestDto createUploadRequestDto)
        {
            string newId = Guid.NewGuid().ToString();
            string objectKey = $"upload-{newId}";

            UploadRequestActiveDto uploadRequestActiveDto = new UploadRequestActiveDto
            {
                CustomerId = createUploadRequestDto.CustomerId,
                EmployeeId = createUploadRequestDto.EmployeeId,
                EmployeeName = createUploadRequestDto.EmployeeName,
                CreatedDate = createUploadRequestDto.CreatedDate,
                Id = newId,
                ObjectKey = objectKey,
                status = UploadStatus.unused,
                ValidUntilDate = DateTime.UtcNow.AddHours(1),
            };

            IdRequest request = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");

            if (_repository.DoesActiveRequestExistForUser(uploadRequestActiveDto.CustomerId))
            {


                //remove old request  
                IdRequest? idRequest = _repository.GetIdRequestForUser(uploadRequestActiveDto.CustomerId);

                UploadRequestActiveDto uploadRequestActiveDtoOld = await encryption.DecryptIdRequest(idRequest, "kek-standard");
                uploadRequestActiveDtoOld.status = UploadStatus.failed;
                IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDtoOld, "kek-standard");
                idRequest.status = updatedRequest.status;
                _repository.UpdateIdRequest(updatedRequest);


                //reset request
                //return null;
            }
            
            _repository.CreateIdRequest(request);

            string url = $"http://localhost:3000/upload/{request.Id}"; ;

            return url;

        }

        public string GenerateMinioUploadUrl(string objectKey)
        {
            using (var s3Client = new AmazonS3Client(accessKey, secretKey, s3Config))
            {
                var request = new Amazon.S3.Model.GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    Verb = Amazon.S3.HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                string url = s3Client.GetPreSignedURL(request);
                return url;
            }
        }

        //check if the token can be used to generate a minio url
        //generate the minio url for the frontend to upload the image to
        public async Task<string?> checkTokenAndGenerateMinioUrl(string token)
        {
            IdRequest? idRequest = _repository.TryGetObjectKeyForId(token);

            if (idRequest == null)
            {
                return null;
            }

            UploadRequestActiveDto uploadRequestActiveDto = await encryption.DecryptIdRequest(idRequest, "kek-standard");

            if (uploadRequestActiveDto.ValidUntilDate < DateTime.UtcNow || (uploadRequestActiveDto.status != UploadStatus.pending&& uploadRequestActiveDto.status != UploadStatus.unused))
            {
                return null;
            }

            uploadRequestActiveDto.status = UploadStatus.pending;
            IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");

            idRequest.status = updatedRequest.status;
            

            _repository.UpdateIdRequest(updatedRequest);
    
            string minioUrl = GenerateMinioUploadUrl(uploadRequestActiveDto.ObjectKey);

            return minioUrl;
        }

        //when minio upload is complete and the frontend recieved the confimation, update the status of the request to pending
        public async Task<bool> ConfirmUpload(string token)
        {
            IdRequest? idRequest = _repository.TryGetObjectKeyForId(token);
            if (idRequest == null)
            {
                return false;
            }
            UploadRequestActiveDto uploadRequestActiveDto = await encryption.DecryptIdRequest(idRequest, "kek-standard");
            if (uploadRequestActiveDto.ValidUntilDate < DateTime.UtcNow || uploadRequestActiveDto.status != UploadStatus.pending)
            {
                return false;
            }
            uploadRequestActiveDto.status = UploadStatus.completed;
            IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");
            idRequest.status = updatedRequest.status;
            _repository.UpdateIdRequest(updatedRequest);
            return true;
        }

        public async Task<bool> UploadImageAsync(Stream fileStream, string id, string contentType)
        {
            try
            {
                IdRequest? idRequest = _repository.TryGetObjectKeyForId(id);
                if (idRequest == null)
                {
                    return false;
                }
                UploadRequestActiveDto uploadRequestActiveDto = await encryption.DecryptIdRequest(idRequest, "kek-standard");
                if (uploadRequestActiveDto.ValidUntilDate < DateTime.UtcNow || (uploadRequestActiveDto.status != UploadStatus.pending && uploadRequestActiveDto.status != UploadStatus.unused))
                {
                    
                    return false;
                }

                using (var client = new AmazonS3Client(accessKey, secretKey, s3Config))
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = $"test-folder/{uploadRequestActiveDto.ObjectKey}",
                        InputStream = fileStream,
                        ContentType = contentType
                    };
                    PutObjectResponse? response=null;
                    try
                    {
                        response = await client.PutObjectAsync(putRequest);

                        uploadRequestActiveDto.status = UploadStatus.completed;
                        IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");
                        idRequest.status = updatedRequest.status;
                        _repository.UpdateIdRequest(updatedRequest);
                    }
                    catch (AmazonS3Exception e)
                    {
                        Console.WriteLine($"S3 Error: {e.Message}");
                        Console.WriteLine($"S3 Error Code: {e.ErrorCode}");

                        uploadRequestActiveDto.status = UploadStatus.failed;
                        IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");
                        idRequest.status = updatedRequest.status;
                        _repository.UpdateIdRequest(updatedRequest);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"General Error: {e.Message}");

                        uploadRequestActiveDto.status = UploadStatus.failed;
                        IdRequest updatedRequest = await encryption.EncryptIdRequest(uploadRequestActiveDto, "kek-standard");
                        idRequest.status = updatedRequest.status;
                        _repository.UpdateIdRequest(updatedRequest);
                    }

                    if(response == null)
                    {
                        return false;
                    }

                    return response.HttpStatusCode == System.Net.HttpStatusCode.OK;

                }
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        public async Task<(Stream? stream, string? contentType, bool expired)> GetImageAsync(string customerId)
        {
            IdRequest? idRequest = _repository.GetIdRequestForUser(customerId);

            if (idRequest == null)
                return (null, null, false);

            UploadRequestActiveDto dto = await encryption.DecryptIdRequest(idRequest, "kek-standard");

            if (dto.status != UploadStatus.completed)
                return (null, null, false);

            using var client = new AmazonS3Client(accessKey, secretKey, s3Config);

            try
            {
                var response = await client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = $"test-folder/{dto.ObjectKey}"
                });

                return (response.ResponseStream, response.Headers.ContentType, false);
            }
            catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (null, null, true);
            }
        }


    }
}
