using System;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    [TestFixture]
    public class FadeEffectTests
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
        public void GivenNewFadeEffect_WhenConstructed_ThenIsActiveIsFalse()
            => Assert.That(new FadeEffect().IsActive, Is.False);

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new FadeEffect().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenIsDisposedIsFalse()
            => Assert.That(new FadeEffect().IsDisposed, Is.False);

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenCurrentMultiplierIsZero()
            => Assert.That(new FadeEffect().CurrentMultiplier, Is.EqualTo(0.0f));

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenIsIncreasingIsTrue()
            => Assert.That(new FadeEffect().IsIncreasing);

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenSpeedIsOne()
            => Assert.That(new FadeEffect().Speed, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenMinimumMultiplierIsZero()
            => Assert.That(new FadeEffect().MinimumMultiplier, Is.EqualTo(0.0f));

        [Test]
        public void GivenNewFadeEffect_WhenConstructed_ThenMaximumMultiplierIsOne()
            => Assert.That(new FadeEffect().MaximumMultiplier, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewFadeEffect_WhenLoadContentIsCalledTwice_ThenThrowsInvalidOperationException()
        {
            FadeEffect fadeEffect = new();
            fadeEffect.LoadContent(sprite);

            Assert.Throws<InvalidOperationException>(() => fadeEffect.LoadContent(sprite));
        }

        [Test]
        public void GivenUnloadedFadeEffect_WhenUnloadContentIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new FadeEffect().UnloadContent());

        [Test]
        public void GivenUnloadedFadeEffect_WhenActivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new FadeEffect().Activate());

        [Test]
        public void GivenUnloadedFadeEffect_WhenDeactivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new FadeEffect().Deactivate());

        [Test]
        public void GivenUnloadedFadeEffect_WhenUpdateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new FadeEffect().Update(null));

        [Test]
        public void GivenLoadedFadeEffect_WhenActivateIsCalled_ThenIsActiveIsTrue()
        {
            FadeEffect fadeEffect = new();
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            Assert.That(fadeEffect.IsActive);
        }

        [Test]
        public void GivenActivatedFadeEffect_WhenDeactivateIsCalled_ThenIsActiveIsFalse()
        {
            FadeEffect fadeEffect = new();
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();
            fadeEffect.Deactivate();

            Assert.That(fadeEffect.IsActive, Is.False);
        }

        [Test]
        public void GivenLoadedFadeEffect_WhenUnloadContentIsCalled_ThenIsContentLoadedIsFalse()
        {
            FadeEffect fadeEffect = new();
            fadeEffect.LoadContent(sprite);
            fadeEffect.UnloadContent();

            Assert.That(fadeEffect.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenActivatedFadeEffectIncreasing_WhenUpdatedForHalfSecond_ThenCurrentMultiplierIncreases()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.0f, IsIncreasing = true };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(fadeEffect.CurrentMultiplier, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedFadeEffectDecreasing_WhenUpdatedForHalfSecond_ThenCurrentMultiplierDecreases()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, IsIncreasing = false };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(fadeEffect.CurrentMultiplier, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedFadeEffectAtMaximum_WhenUpdated_ThenCurrentMultiplierClampsToMaximum()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MaximumMultiplier = 1.0f, IsIncreasing = true };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(fadeEffect.CurrentMultiplier, Is.EqualTo(1.0f));
        }

        [Test]
        public void GivenActivatedFadeEffectAtMaximum_WhenUpdated_ThenDirectionReversesToDecreasing()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 1.0f, MaximumMultiplier = 1.0f, IsIncreasing = true };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(fadeEffect.IsIncreasing, Is.False);
        }

        [Test]
        public void GivenActivatedFadeEffectAtMinimum_WhenUpdated_ThenCurrentMultiplierClampsToMinimum()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(fadeEffect.CurrentMultiplier, Is.EqualTo(0.0f));
        }

        [Test]
        public void GivenActivatedFadeEffectAtMinimum_WhenUpdated_ThenDirectionReversesToIncreasing()
        {
            FadeEffect fadeEffect = new() { Speed = 1.0f, CurrentMultiplier = 0.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            fadeEffect.LoadContent(sprite);
            fadeEffect.Activate();

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(fadeEffect.IsIncreasing);
        }

        [Test]
        public void GivenLoadedButInactiveFadeEffect_WhenUpdated_ThenCurrentMultiplierDoesNotChange()
        {
            FadeEffect fadeEffect = new() { CurrentMultiplier = 0.5f };
            fadeEffect.LoadContent(sprite);

            fadeEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(fadeEffect.CurrentMultiplier, Is.EqualTo(0.5f));
        }
    }
}
