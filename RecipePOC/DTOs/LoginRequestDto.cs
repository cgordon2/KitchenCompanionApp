using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DTOs
{
    public record struct LoginRequestDto(string Username, string Password);
}
