using System;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    [TestFixture]
    public class ZoomEffectTests
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
        public void GivenNewZoomEffect_WhenConstructed_ThenIsActiveIsFalse()
            => Assert.That(new ZoomEffect().IsActive, Is.False);

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new ZoomEffect().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenCurrentHorizontalMultiplierIsOne()
            => Assert.That(new ZoomEffect().CurrentHorizontalMultiplier, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenCurrentVerticalMultiplierIsOne()
            => Assert.That(new ZoomEffect().CurrentVerticalMultiplier, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenIsIncreasingIsTrue()
            => Assert.That(new ZoomEffect().IsIncreasing);

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenMinimumMultiplierIsZero()
            => Assert.That(new ZoomEffect().MinimumMultiplier, Is.EqualTo(0.0f));

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenMaximumMultiplierIsTwo()
            => Assert.That(new ZoomEffect().MaximumMultiplier, Is.EqualTo(2.0f));

        [Test]
        public void GivenActivatedZoomEffectIncreasing_WhenUpdated_ThenHorizontalAndVerticalMultipliersAreEqual()
        {
            ZoomEffect zoomEffect = new() { Speed = 0.5f, CurrentHorizontalMultiplier = 1.0f, IsIncreasing = true };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(zoomEffect.CurrentVerticalMultiplier));
        }

        [Test]
        public void GivenActivatedZoomEffectIncreasing_WhenUpdated_ThenHorizontalMultiplierIncreases()
        {
            ZoomEffect zoomEffect = new() { Speed = 0.5f, CurrentHorizontalMultiplier = 1.0f, MaximumMultiplier = 2.0f, IsIncreasing = true };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(1.5f).Within(0.001f));
        }

        [Test]
        public void GivenActivatedZoomEffectAtMaximum_WhenUpdated_ThenHorizontalMultiplierClampsToMaximum()
        {
            ZoomEffect zoomEffect = new() { Speed = 1.0f, CurrentHorizontalMultiplier = 2.0f, MaximumMultiplier = 2.0f, IsIncreasing = true };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(2.0f));
        }

        [Test]
        public void GivenActivatedZoomEffectAtMaximum_WhenUpdated_ThenDirectionReversesToDecreasing()
        {
            ZoomEffect zoomEffect = new() { Speed = 1.0f, CurrentHorizontalMultiplier = 2.0f, MaximumMultiplier = 2.0f, IsIncreasing = true };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(zoomEffect.IsIncreasing, Is.False);
        }

        [Test]
        public void GivenActivatedZoomEffectAtMinimum_WhenUpdated_ThenHorizontalMultiplierClampsToMinimum()
        {
            ZoomEffect zoomEffect = new() { Speed = 1.0f, CurrentHorizontalMultiplier = 0.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.5)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(0.0f));
        }

        [Test]
        public void GivenActivatedZoomEffectAtMinimum_WhenUpdated_ThenDirectionReversesToIncreasing()
        {
            ZoomEffect zoomEffect = new() { Speed = 1.0f, CurrentHorizontalMultiplier = 0.0f, MinimumMultiplier = 0.0f, IsIncreasing = false };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));

            Assert.That(zoomEffect.IsIncreasing);
        }

        [Test]
        public void GivenLoadedButInactiveZoomEffect_WhenUpdated_ThenHorizontalMultiplierDoesNotChange()
        {
            ZoomEffect zoomEffect = new() { CurrentHorizontalMultiplier = 1.5f };
            zoomEffect.LoadContent(sprite);

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(1.5f));
        }

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenIsDisposedIsFalse()
            => Assert.That(new ZoomEffect().IsDisposed, Is.False);

        [Test]
        public void GivenNewZoomEffect_WhenConstructed_ThenSpeedIs0Point1()
            => Assert.That(new ZoomEffect().Speed, Is.EqualTo(0.1f));

        [Test]
        public void GivenUnloadedZoomEffect_WhenActivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new ZoomEffect().Activate());

        [Test]
        public void GivenUnloadedZoomEffect_WhenDeactivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new ZoomEffect().Deactivate());

        [Test]
        public void GivenUnloadedZoomEffect_WhenUpdateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new ZoomEffect().Update(null));

        [Test]
        public void GivenZoomEffect_WhenLoadContentCalledTwice_ThenThrowsInvalidOperationException()
        {
            ZoomEffect zoomEffect = new();
            zoomEffect.LoadContent(sprite);

            Assert.Throws<InvalidOperationException>(() => zoomEffect.LoadContent(sprite));
        }

        [Test]
        public void GivenLoadedZoomEffect_WhenActivated_ThenIsActiveIsTrue()
        {
            ZoomEffect zoomEffect = new();
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            Assert.That(zoomEffect.IsActive);
        }

        [Test]
        public void GivenActivatedZoomEffect_WhenDeactivated_ThenIsActiveIsFalse()
        {
            ZoomEffect zoomEffect = new();
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();
            zoomEffect.Deactivate();

            Assert.That(zoomEffect.IsActive, Is.False);
        }

        [Test]
        public void GivenActivatedZoomEffectDecreasing_WhenUpdated_ThenHorizontalAndVerticalMultipliersAreEqual()
        {
            ZoomEffect zoomEffect = new()
            {
                Speed = 0.5f,
                CurrentHorizontalMultiplier = 1.0f,
                MinimumMultiplier = 0.0f,
                IsIncreasing = false
            };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(zoomEffect.CurrentVerticalMultiplier));
        }

        [Test]
        public void GivenActivatedZoomEffectDecreasing_WhenUpdated_ThenHorizontalMultiplierDecreases()
        {
            ZoomEffect zoomEffect = new()
            {
                Speed = 0.5f,
                CurrentHorizontalMultiplier = 1.0f,
                MinimumMultiplier = 0.0f,
                IsIncreasing = false
            };
            zoomEffect.LoadContent(sprite);
            zoomEffect.Activate();

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentHorizontalMultiplier, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void GivenLoadedButInactiveZoomEffect_WhenUpdated_ThenVerticalMultiplierDoesNotChange()
        {
            ZoomEffect zoomEffect = new() { CurrentVerticalMultiplier = 1.5f };
            zoomEffect.LoadContent(sprite);

            zoomEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(zoomEffect.CurrentVerticalMultiplier, Is.EqualTo(1.5f));
        }
    }
}
