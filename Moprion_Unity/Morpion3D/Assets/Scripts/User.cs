using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClient.Models
{

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public User(int id, string username)
        {
            this.Id = id;
            this.UserName = username;
        }

        public void Display()
        {
            Console.WriteLine($"UserName: {UserName}, Id:{Id}");
        }

    }
}
