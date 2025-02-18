using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountManagement.Web.Controllers.AccountManagement
{
    public class RegistrationController : Controller
    {
        [Route("Registration")]
        public ActionResult Index()
        {
            var employees = new List<RegistrationDTO>
        {
            new RegistrationDTO { OrgId = 1, Name = "Amazon", Size = "Small" },
            new RegistrationDTO { OrgId = 2, Name = "Tesco", Size = "Large" }
        };

            var viewModel = new DataGridViewModel
            {
                Data = employees.Cast<object>().ToList()
            };

            return View("DataGridView", viewModel);
        }
    }

    public class RegistrationDTO
    {
        public int OrgId { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
    }


    [Route("Accreditation")]
    public class AccreditationController : Controller
    {
        public ActionResult Index()
        {
            var products = new List<AccreditationDTO>
        {
            new AccreditationDTO { OrgId = 101, Name = "Amazon", Revenue = 899.99M, PackagingType = "Cardboard" },
            new AccreditationDTO { OrgId = 102, Name = "Tesco", Revenue = 29.99M, PackagingType = "Plastic" }
        };

            var viewModel = new DataGridViewModel
            {
                Data = products.Cast<object>().ToList()
            };

            return View("DataGridView", viewModel);
        }
    }

    public class AccreditationDTO
    {
        public int OrgId { get; set; }
        public string Name { get; set; }
        public decimal Revenue { get; set; }
        public string PackagingType { get; set; }
    }

    public class DataGridViewModel
    {
        public IEnumerable<object> Data { get; set; } = new List<object>();

        public List<string> Columns
        {
            get
            {
                if (Data == null || !Data.Any())
                    return new List<string>();

                var firstItem = Data.First();
                return firstItem.GetType().GetProperties().Select(p => p.Name).ToList();
            }
        }
    }

}
