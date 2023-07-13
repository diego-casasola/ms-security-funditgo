using Microsoft.AspNetCore.Identity;
using SharedKernel.Core;

namespace Security.Infrastructure.Security
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public DateTime FechaNacimiento{ get; set; }
        public bool Active { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public bool Staff { get; set; }

        public ApplicationUser(string username, string firstName, string lastName, string email, DateTime fechaNacimiento, bool active, bool staff) : base(username)
        {

            if (GetAge(fechaNacimiento)<18)
            {
                throw new BussinessRuleValidationException("Es necesario que el usuario sea mayor de edad");
            }

            LastName = lastName;
            FirstName = firstName;
            Email = email;
            FechaNacimiento = fechaNacimiento;
            Active = active;
            Staff = staff;
        }

        private ApplicationUser()
        {
            LastName = "";
            FirstName = "";
        }
        private Int32 GetAge(DateTime fechaNacimiento)
        {
            var today = DateTime.Today;

            var a = (today.Year * 100 + today.Month) * 100 + today.Day;
            var b = (fechaNacimiento.Year * 100 + fechaNacimiento.Month) * 100 + fechaNacimiento.Day;

            return (a - b) / 10000;
        }
    }

    public class ApplicationRole : IdentityRole<Guid> { }
}
