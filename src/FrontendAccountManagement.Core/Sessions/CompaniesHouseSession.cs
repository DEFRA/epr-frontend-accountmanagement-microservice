﻿using FrontendAccountManagement.Core.Models.CompaniesHouse;

namespace FrontendAccountManagement.Core.Sessions;

public record CompaniesHouseSession
{
    public CompaniesHouseResponse CompaniesHouseResponse { get; set; } = new CompaniesHouseResponse();
}