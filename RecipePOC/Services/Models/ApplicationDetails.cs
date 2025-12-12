using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.Services.Models
{
    public readonly record struct ApplicationDetails(string Name, string Version, DateTime LastUpdatedOn);

}
