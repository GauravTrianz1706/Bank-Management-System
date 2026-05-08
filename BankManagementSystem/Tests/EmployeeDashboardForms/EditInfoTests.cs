using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.EmployeeDashboardForms;

namespace BankManagementSystem.Tests.EmployeeDashboardForms
{
    public class EditInfoTests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new EditInfo();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void EditInfo_IsFormType()
        {
            // Act
            var form = new EditInfo();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void EditInfo_CanBeInstantiated()
        {
            // Act
            Action act = () => new EditInfo();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_DoesNotThrowException()
        {
            // Act
            Action act = () => new EditInfo();

            // Assert
            act.Should().NotThrow();
        }
    }
}
