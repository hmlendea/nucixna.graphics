using System;

using Microsoft.Xna.Framework;
using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

namespace NuciXNA.Graphics.UnitTests.SpriteEffects
{
    public class MovementEffectTests
    {
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
            TextSprite sprite = new();
            sprite.LoadContent();
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            Assert.That(movementEffect.IsActive, Is.True);
        }

        [Test]
        public void GivenLoadedMovementEffect_WhenActivated_ThenLocationOffsetIsEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100) };
            TextSprite sprite = new();
            sprite.LoadContent();
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            Assert.That(movementEffect.LocationOffset, Is.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenActivatedMovementEffectWithDistantTarget_WhenUpdated_ThenLocationOffsetChanges()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 10.0f };
            TextSprite sprite = new() { Location = Point2D.Empty };
            sprite.LoadContent();
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.LocationOffset, Is.Not.EqualTo(Point2D.Empty));
        }

        [Test]
        public void GivenActivatedMovementEffectWithDistantTarget_WhenUpdated_ThenEffectRemainsActive()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 5.0f };
            TextSprite sprite = new() { Location = Point2D.Empty };
            sprite.LoadContent();
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.IsActive, Is.True);
        }

        [Test]
        public void GivenActivatedMovementEffectReachingTarget_WhenUpdated_ThenEffectIsDeactivated()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(5, 0), Speed = 100.0f };
            TextSprite sprite = new() { Location = Point2D.Empty };
            sprite.LoadContent();
            movementEffect.LoadContent(sprite);
            movementEffect.Activate();

            movementEffect.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0)));

            Assert.That(movementEffect.IsActive, Is.False);
        }

        [Test]
        public void GivenActivatedMovementEffect_WhenDeactivated_ThenLocationOffsetIsEmpty()
        {
            MovementEffect movementEffect = new() { TargetLocation = new Point2D(100, 100), Speed = 10.0f };
            TextSprite sprite = new() { Location = Point2D.Empty };
            sprite.LoadContent();
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
            TextSprite sprite = new() { Location = Point2D.Empty };
            sprite.LoadContent();
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
    }
}
