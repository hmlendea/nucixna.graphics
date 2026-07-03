using System;

using Microsoft.Xna.Framework;
using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    public class OscilationEffectTests
    {
        TextSprite _sprite;

        [SetUp]
        public void SetUp()
        {
            _sprite = new TextSprite();
            _sprite.LoadContent();
        }

        [TearDown]
        public void TearDown()
        {
            _sprite.Dispose();
        }

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenIsActiveIsFalse()
            => Assert.That(new OscilationEffect().IsActive, Is.False);

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new OscilationEffect().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenCurrentMultiplierIsOne()
            => Assert.That(new OscilationEffect().CurrentMultiplier, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenIsIncreasingIsTrue()
            => Assert.That(new OscilationEffect().IsIncreasing);

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenMinimumMultiplierIsHalf()
            => Assert.That(new OscilationEffect().MinimumMultiplier, Is.EqualTo(0.5f));

        [Test]
        public void GivenNewOscilationEffect_WhenConstructed_ThenMaximumMultiplierIs1Point5()
            => Assert.That(new OscilationEffect().MaximumMultiplier, Is.EqualTo(1.5f));

        [Test]
        public void GivenActivatedOscilationEffectIncreasing_WhenUpdated_ThenCurrentMultiplierIncreases()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MaximumMultiplier = 2.0f, IsIncreasing = true };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)));

            Assert.That(oscilationEffect.CurrentMultiplier, Is.EqualTo(1.2f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedOscilationEffectDecreasing_WhenUpdated_ThenCurrentMultiplierDecreases()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)));

            Assert.That(oscilationEffect.CurrentMultiplier, Is.EqualTo(0.8f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedOscilationEffectAtMaximum_WhenUpdated_ThenCurrentMultiplierClampsToMaximum()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.5f, MaximumMultiplier = 1.5f, IsIncreasing = true };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(oscilationEffect.CurrentMultiplier, Is.EqualTo(1.5f));
        }

        [Test]
        public void GivenActivatedOscilationEffectAtMaximum_WhenUpdated_ThenDirectionReversesToDecreasing()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.5f, MaximumMultiplier = 1.5f, IsIncreasing = true };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(oscilationEffect.IsIncreasing, Is.False);
        }

        [Test]
        public void GivenActivatedOscilationEffectAtMinimum_WhenUpdated_ThenCurrentMultiplierClampsToMinimum()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.5f, MinimumMultiplier = 0.5f, IsIncreasing = false };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(oscilationEffect.CurrentMultiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void GivenActivatedOscilationEffectAtMinimum_WhenUpdated_ThenDirectionReversesToIncreasing()
        {
            OscilationEffect oscilationEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.5f, MinimumMultiplier = 0.5f, IsIncreasing = false };
            oscilationEffect.LoadContent(_sprite);
            oscilationEffect.Activate();

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(oscilationEffect.IsIncreasing);
        }

        [Test]
        public void GivenLoadedButInactiveOscilationEffect_WhenUpdated_ThenCurrentMultiplierDoesNotChange()
        {
            OscilationEffect oscilationEffect = new() { CurrentMultiplier = 1.2f };
            oscilationEffect.LoadContent(_sprite);

            oscilationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(oscilationEffect.CurrentMultiplier, Is.EqualTo(1.2f));
        }
    }
}
