using System.ServiceModel;
using WebServicesEnrollment.Models;

namespace WebServicesEnrollment.Service
{
    [ServiceContract]
        public interface IEnrollmentService
        {
            [OperationContract]
            string Test(string s);
            [OperationContract]
            EnrollmentResponse EnrollmentProcess(EnrollmentRequest request);
        }
    }