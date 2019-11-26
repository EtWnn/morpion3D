using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClient.Models
{
    /// <summary>
    /// Users describe the clients connected to the server
    /// </summary>
    public class User
    {
        public int Id {get; set;}
        public string UserName { get; set; }

        /// <summary>
        /// Initialize an instance with an <paramref name="id"/> and a <paramref name="username"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        public User(int id, string username)
        {
            this.Id = id;
            this.UserName = username;
        }

        /// <summary>
        /// Display the attributes of a User
        /// </summary>
        public void Display()
        {
            Console.WriteLine($"UserName: {UserName}, Id:{Id}");
        }

    }
}
