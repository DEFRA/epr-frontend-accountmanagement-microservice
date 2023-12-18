using FluentAssertions;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using System;

namespace FrontendAccountManagement.Core.UnitTests.Models
{
    [TestClass]
    public class CurrentUserAccessTests
    {
        private readonly CurrentUserAccess _currentUserAccess = new();

        [TestInitialize]
        public void TestInitialize()
        {
            _currentUserAccess.IsApprovedPerson = false;
        }

        [TestMethod]
        public void SetAsApproved_ReturnsTrue()
        {
            // Arrange
            _currentUserAccess.IsApprovedPerson = true;

            // Act
            var result = _currentUserAccess.IsApprovedPerson;

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void SetAsNotApproved_ReturnsFalse()
        {
            // Arrange
            _currentUserAccess.IsApprovedPerson = false;

            // Act
            var result = _currentUserAccess.IsApprovedPerson;

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetPermissionType_ReturnsNotSet()
        {
            // Arrange
            var testPermission = PermissionType.NotSet;
            _currentUserAccess.PermissionType = testPermission;

            // Act
            var result = _currentUserAccess.PermissionType;

            // Assert
            result.Should().Be(testPermission);
        }

        [TestMethod]
        public void SetPermissionType_ReturnsValueSet()
        {
            // Arrange
            var testPermission = PermissionType.Admin;
            _currentUserAccess.PermissionType = testPermission;

            // Act
            var result = _currentUserAccess.PermissionType;

            // Assert
            result.Should().Be(testPermission);
        }
    }
}
