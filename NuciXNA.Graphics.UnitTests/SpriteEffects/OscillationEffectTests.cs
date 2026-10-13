using System;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    [TestFixture]
    public class OscillationEffectTests
    {
        private TextSprite sprite;

        [SetUp]
        public void SetUp()
        {
            sprite = new TextSprite();
            sprite.LoadContent();
        }

        [TearDown]
        public void TearDown()
        {
            sprite.Dispose();
        }

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenIsActiveIsFalse()
            => Assert.That(new OscillationEffect().IsActive, Is.False);

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new OscillationEffect().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenCurrentMultiplierIsOne()
            => Assert.That(new OscillationEffect().CurrentMultiplier, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenIsIncreasingIsTrue()
            => Assert.That(new OscillationEffect().IsIncreasing);

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenMinimumMultiplierIsHalf()
            => Assert.That(new OscillationEffect().MinimumMultiplier, Is.EqualTo(0.5f));

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenMaximumMultiplierIs1Point5()
            => Assert.That(new OscillationEffect().MaximumMultiplier, Is.EqualTo(1.5f));

        [Test]
        public void GivenActivatedOscillationEffectIncreasing_WhenUpdated_ThenCurrentMultiplierIncreases()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MaximumMultiplier = 2.0f, IsIncreasing = true };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)));

            Assert.That(oscillationEffect.CurrentMultiplier, Is.EqualTo(1.2f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedOscillationEffectDecreasing_WhenUpdated_ThenCurrentMultiplierDecreases()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.2)));

            Assert.That(oscillationEffect.CurrentMultiplier, Is.EqualTo(0.8f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedOscillationEffectAtMaximum_WhenUpdated_ThenCurrentMultiplierClampsToMaximum()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.5f, MaximumMultiplier = 1.5f, IsIncreasing = true };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(oscillationEffect.CurrentMultiplier, Is.EqualTo(1.5f));
        }

        [Test]
        public void GivenActivatedOscillationEffectAtMaximum_WhenUpdated_ThenDirectionReversesToDecreasing()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.5f, MaximumMultiplier = 1.5f, IsIncreasing = true };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(oscillationEffect.IsIncreasing, Is.False);
        }

        [Test]
        public void GivenActivatedOscillationEffectAtMinimum_WhenUpdated_ThenCurrentMultiplierClampsToMinimum()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.5f, MinimumMultiplier = 0.5f, IsIncreasing = false };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(oscillationEffect.CurrentMultiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void GivenActivatedOscillationEffectAtMinimum_WhenUpdated_ThenDirectionReversesToIncreasing()
        {
            OscillationEffect oscillationEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.5f, MinimumMultiplier = 0.5f, IsIncreasing = false };
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(oscillationEffect.IsIncreasing);
        }

        [Test]
        public void GivenLoadedButInactiveOscillationEffect_WhenUpdated_ThenCurrentMultiplierDoesNotChange()
        {
            OscillationEffect oscillationEffect = new() { CurrentMultiplier = 1.2f };
            oscillationEffect.LoadContent(sprite);

            oscillationEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(oscillationEffect.CurrentMultiplier, Is.EqualTo(1.2f));
        }

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenIsDisposedIsFalse()
            => Assert.That(new OscillationEffect().IsDisposed, Is.False);

        [Test]
        public void GivenNewOscillationEffect_WhenConstructed_ThenSpeedIsOne()
            => Assert.That(new OscillationEffect().Speed, Is.EqualTo(1.0f));

        [Test]
        public void GivenUnloadedOscillationEffect_WhenActivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new OscillationEffect().Activate());

        [Test]
        public void GivenUnloadedOscillationEffect_WhenDeactivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new OscillationEffect().Deactivate());

        [Test]
        public void GivenUnloadedOscillationEffect_WhenUpdateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new OscillationEffect().Update(null));

        [Test]
        public void GivenOscillationEffect_WhenLoadContentCalledTwice_ThenThrowsInvalidOperationException()
        {
            OscillationEffect oscillationEffect = new();
            oscillationEffect.LoadContent(sprite);

            Assert.Throws<InvalidOperationException>(() => oscillationEffect.LoadContent(sprite));
        }

        [Test]
        public void GivenLoadedOscillationEffect_WhenActivated_ThenIsActiveIsTrue()
        {
            OscillationEffect oscillationEffect = new();
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();

            Assert.That(oscillationEffect.IsActive);
        }

        [Test]
        public void GivenActivatedOscillationEffect_WhenDeactivated_ThenIsActiveIsFalse()
        {
            OscillationEffect oscillationEffect = new();
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();
            oscillationEffect.Deactivate();

            Assert.That(oscillationEffect.IsActive, Is.False);
        }

        [Test]
        public void GivenLoadedOscillationEffect_WhenUnloaded_ThenIsContentLoadedIsFalse()
        {
            OscillationEffect oscillationEffect = new();
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.UnloadContent();

            Assert.That(oscillationEffect.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenActivatedOscillationEffect_WhenUnloaded_ThenIsActiveIsFalse()
        {
            OscillationEffect oscillationEffect = new();
            oscillationEffect.LoadContent(sprite);
            oscillationEffect.Activate();
            oscillationEffect.UnloadContent();

            Assert.That(oscillationEffect.IsActive, Is.False);
        }
    }
}
