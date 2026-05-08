using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem.EmployeeDashboardForms;

namespace BankManagementSystem.Tests.EmployeeDashboardForms
{
    public class DepositTests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new Deposit();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void Deposit_IsFormType()
        {
            // Act
            var form = new Deposit();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void Deposit_CanBeInstantiated()
        {
            // Act
            Action act = () => new Deposit();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Constructor_DoesNotThrowException()
        {
            // Act
            Action act = () => new Deposit();

            // Assert
            act.Should().NotThrow();
        }
    }
}
