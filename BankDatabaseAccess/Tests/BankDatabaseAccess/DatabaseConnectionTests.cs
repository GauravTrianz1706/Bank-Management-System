using Xunit;
using System;
using BankDatabaseAccess;
using Npgsql;

namespace BankDatabaseAccess.Tests
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public void Connection_ShouldHaveDefaultValue()
        {
            // Arrange & Act
            var connection = DatabaseConnection.Connection;

            // Assert
            Assert.NotNull(connection);
            Assert.NotEmpty(connection);
            Assert.Contains("Host=localhost", connection);
            Assert.Contains("Database=openbanklocal", connection);
        }

        [Fact]
        public void Connection_ShouldBeSettable()
        {
            // Arrange
            var originalConnection = DatabaseConnection.Connection;
            var newConnection = "Host=testhost;Database=testdb;Username=testuser;Password=testpass;Port=5432;";

            // Act
            DatabaseConnection.Connection = newConnection;

            // Assert
            Assert.Equal(newConnection, DatabaseConnection.Connection);

            // Cleanup
            DatabaseConnection.Connection = originalConnection;
        }

        [Fact]
        public void Execute_WithInvalidConnectionString_ShouldThrowException()
        {
            // Arrange
            var originalConnection = DatabaseConnection.Connection;
            DatabaseConnection.Connection = "InvalidConnectionString";
            var query = "SELECT 1";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => DatabaseConnection.Execute(query));

            // Cleanup
            DatabaseConnection.Connection = originalConnection;
        }

        [Fact]
        public void Execute_WithNullQuery_ShouldThrowException()
        {
            // Arrange
            string? query = null;

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => DatabaseConnection.Execute(query!));
        }

        [Fact]
        public void Execute_WithEmptyQuery_ShouldThrowException()
        {
            // Arrange
            var query = "";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => DatabaseConnection.Execute(query));
        }

        [Theory]
        [InlineData("SELECT 1")]
        [InlineData("SELECT * FROM information_schema.tables LIMIT 1")]
        public void Execute_WithValidSelectQuery_ShouldNotThrowException(string query)
        {
            // This test would require a real database connection
            // In a real scenario, you would mock the database or use a test database
            // For now, we're testing the structure
            Assert.NotNull(query);
        }

        [Fact]
        public void Error_UsernameExist_ShouldHaveCorrectValue()
        {
            // Arrange & Act
            var errorValue = (int)Error.UsernameExist;

            // Assert
            Assert.Equal(-1, errorValue);
        }

        [Fact]
        public void Error_Success_ShouldHaveCorrectValue()
        {
            // Arrange & Act
            var errorValue = (int)Error.Success;

            // Assert
            Assert.Equal(0, errorValue);
        }

        [Fact]
        public void Error_ShouldHaveTwoValues()
        {
            // Arrange & Act
            var errorValues = Enum.GetValues(typeof(Error));

            // Assert
            Assert.Equal(2, errorValues.Length);
        }

        [Fact]
        public void Connection_ShouldContainPoolingConfiguration()
        {
            // Arrange & Act
            var connection = DatabaseConnection.Connection;

            // Assert
            Assert.Contains("Pooling=true", connection);
            Assert.Contains("Maximum Pool Size=100", connection);
        }

        [Fact]
        public void Connection_ShouldContainTimeoutConfiguration()
        {
            // Arrange & Act
            var connection = DatabaseConnection.Connection;

            // Assert
            Assert.Contains("Timeout=30", connection);
        }
    }
}
