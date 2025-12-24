using System;

using Auth_util;

namespace Profile_Service;

public static class ProfileService
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static User? Profile_User(string username, string password, string token)
    {
        bool DataValue = Auth.Auth_User(token);

        if (DataValue)
        {
            Console.WriteLine("User data activ");
            return new User
            {
                
            };
        } else
        {
            Console.WriteLine("User data maybe activ");
            return new User
            {
                
            };   
        }
    }
}