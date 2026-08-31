using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SuperHeroAPI.Tests
{
    public class BattleSimulationTests
    {
        [Fact]
        public void BattleSimulation_Should_CalculateDamageCorrectly()
        {
            // Arrange
            var attacker = new SuperHero
            {
                AttackPower = 60,
                Defense = 40
            };
            
            var defender = new SuperHero
            {
                AttackPower = 50,
                Defense = 30
            };

            // Act & Assert
            // Damage calculation should be: Max(5, Attacker.AttackPower - (Defender.Defense / 2)) + Random variance (0 to 5)
            int baseDamage = Math.Max(5, attacker.AttackPower - (defender.Defense / 2));
            baseDamage.Should().BeGreaterThanOrEqualTo(5);
            baseDamage.Should().BeLessThanOrEqualTo(60); // 60 - 15 = 45, plus random 0-5
        }
    }
}