using Xunit;
using FluentAssertions;
using System;
using System.Windows.Forms;
using BankManagementSystem;

namespace BankManagementSystem.Tests
{
    public class WelcomeUITests
    {
        [Fact]
        public void Constructor_InitializesForm()
        {
            // Act
            var form = new WelcomeUI();

            // Assert
            form.Should().NotBeNull();
        }

        [Fact]
        public void WelcomeUI_IsFormType()
        {
            // Act
            var form = new WelcomeUI();

            // Assert
            form.Should().BeAssignableTo<Form>();
        }

        [Fact]
        public void WelcomeUI_CanBeInstantiated()
        {
            // Act
            Action act = () => new WelcomeUI();

            // Assert
            act.Should().NotThrow();
        }
    }
}
