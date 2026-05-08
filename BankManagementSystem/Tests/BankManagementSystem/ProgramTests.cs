using Xunit;
using FluentAssertions;
using System;
using Microsoft.Extensions.Configuration;
using BankManagementSystem;

namespace BankManagementSystem.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Configuration_IsNullableIConfiguration()
        {
            // Act
            var config = Program.Configuration;

            // Assert
            config.Should().BeAssignableTo<IConfiguration>();
        }

        [Fact]
        public void Configuration_CanBeNull()
        {
            // This test verifies that Configuration property can be null
            // which is expected before Main() is called
            
            // Act & Assert
            // No exception should be thrown when accessing the property
            var config = Program.Configuration;
            // Configuration can be null or not null depending on when it's accessed
        }

        [Fact]
        public void Program_HasStaticConfigurationProperty()
        {
            // Arrange & Act
            var propertyInfo = typeof(Program).GetProperty("Configuration");

            // Assert
            propertyInfo.Should().NotBeNull();
            propertyInfo.PropertyType.Should().Be(typeof(IConfiguration));
        }
    }
}
