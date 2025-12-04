namespace APi.Models.Role
{
    public class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
        public const string Guest = "Guest";

        public static readonly string[] AllRoles = {SuperAdmin, Admin, Manager, User};
        public static readonly string[] AdminRoles = { SuperAdmin, Admin };
        public static readonly string[] ManagementRoles = { SuperAdmin, Admin, Manager };


        public static bool IsValidRole(string role)
        {
            return AllRoles.Contains(role);
        }
        public static bool IsAdminRole(string role)
        {
            return AdminRoles.Contains(role);
        }
    }
}
