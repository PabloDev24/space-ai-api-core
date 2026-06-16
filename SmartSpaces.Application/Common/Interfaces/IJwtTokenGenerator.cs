using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using SmartSpaces.Domain.Entities;

namespace SmartSpaces.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User user);
        String GenerateRefreshToken();
    }
}
