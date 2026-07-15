using System;

using Microsoft.Xna.Framework;

using NuciXNA.Primitives;

using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    [TestFixture]
    public class MovementEffectTests
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
        public void GivenNewMovementEffect_WhenConstructed_ThenIsActiveIsFalse()
            => Assert.That(new MovementEffect().IsActive, Is.False);

        [Test]
        public void GivenNewMovementEffect_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new MovementEffect().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewMovementEffect_WhenConstructed_ThenLocationOffsetIsEmpty()
            => Assert.That(new MovementEffect().LocationOffset, Is.EqualTo(Point2D.Empty));

        [Test]
        public void GivenNewMovementEffect_WhenConstructed_ThenSpeedIs7Point5()
            => Assert.That(new MovementEffect().Speed, Is.EqualTo(7.5f));

        [Test]
        public void GivenLoadedMovementEffect_WhenActivated_ThenIsActiveIsTrue()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100) };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            Assert.That(movementEffect.IsActive);
        }

        [Test]
        public void GivenLoadedMovementEffect_WhenActivated_ThenLocationOffsetIsEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100) };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenActivatedMovementEffectWithDistantTarget_WhenUpdated_ThenLocationOffsetChanges()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 10.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.LocationOffset, Is.Not.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenActivatedMovementEffectWithDistantTarget_WhenUpdated_ThenEffectRemainsActive()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 5.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.IsActive);
        }

        [Test]
        public void GivenActivatedMovementEffectReachingTarget_WhenUpdated_ThenEffectIsDeactivated()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(5, 0), Speed = 100.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.IsActive, Is.False);
        }

        [Test]
        public void GivenActivatedMovementEffect_WhenDeactivated_ThenLocationOffsetIsEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 10.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();
            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));
            movementEffect.Deactivate();

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenLoadedButInactiveMovementEffect_WhenUpdated_ThenLocationOffsetDoesNotChange()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100) };
            movementEffect.LoadContent(sprite);

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenUnloadedMovementEffect_WhenActivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new MovementEffect().Activate());

        [Test]
        public void GivenUnloadedMovementEffect_WhenDeactivateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new MovementEffect().Deactivate());

        [Test]
        public void GivenUnloadedMovementEffect_WhenUpdateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new MovementEffect().Update(null));

        [Test]
        public void GivenNewMovementEffect_WhenConstructed_ThenIsDisposedIsFalse()
            => Assert.That(new MovementEffect().IsDisposed, Is.False);

        [Test]
        public void GivenNewMovementEffect_WhenUnloadContentIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new MovementEffect().UnloadContent());

        [Test]
        public void GivenMovementEffect_WhenLoadContentCalledTwice_ThenThrowsInvalidOperationException()
        {
            MovementEffect movementEffect = new();
            movementEffect.LoadContent(sprite);

            Assert.Throws<InvalidOperationException>(() => movementEffect.LoadContent(sprite));
        }

        [Test]
        public void GivenActivatedMovementEffectWithOffset_WhenReactivated_ThenLocationOffsetResetsToEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 10.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();
            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));
            movementEffect.Activate();

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenActivatedMovementEffectReachingTarget_WhenUpdated_ThenLocationOffsetIsEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(5, 0), Speed = 100.0f };
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }
    }
}
