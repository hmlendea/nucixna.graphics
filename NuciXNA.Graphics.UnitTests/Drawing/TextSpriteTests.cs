using System;
using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

namespace NuciXNA.Graphics.UnitTests.Drawing
{
    public class TextSpriteTests
    {
        TextSprite _textSprite;

        [SetUp]
        public void SetUp()
        {
            _textSprite = new TextSprite();
        }

        [TearDown]
        public void TearDown()
        {
            _textSprite.Dispose();
        }

        [Test]
        public void GivenLoadedTextSprite_WhenLoadContentIsCalledAgain_ThenThrowsInvalidOperationException()
        {
            _textSprite.LoadContent();

            Assert.Throws<InvalidOperationException>(_textSprite.LoadContent);
        }

        [Test]
        public void GivenNewTextSprite_WhenLoadContentIsCalled_ThenFiresContentLoadingEvent()
        {
            bool eventFired = false;

            _textSprite.ContentLoading += delegate { eventFired = true; };

            _textSprite.LoadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenNewTextSprite_WhenLoadContentIsCalled_ThenFiresContentLoadedEvent()
        {
            bool eventFired = false;

            _textSprite.ContentLoaded += delegate { eventFired = true; };

            _textSprite.LoadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenNewTextSprite_WhenLoadContentIsCalled_ThenContentLoadingFiresBeforeContentLoaded()
        {
            int callOrder = 0;
            int contentLoadingOrder = 0;
            int contentLoadedOrder = 0;

            _textSprite.ContentLoading += delegate { contentLoadingOrder = ++callOrder; };
            _textSprite.ContentLoaded += delegate { contentLoadedOrder = ++callOrder; };

            _textSprite.LoadContent();

            Assert.That(contentLoadingOrder, Is.LessThan(contentLoadedOrder));
        }

        [Test]
        public void GivenNewTextSprite_WhenUnloadContentIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().UnloadContent());

        [Test]
        public void GivenLoadedTextSprite_WhenUnloadContentIsCalled_ThenFiresContentUnloadingEvent()
        {
            bool eventFired = false;

            _textSprite.ContentUnloading += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.UnloadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUnloadContentIsCalled_ThenFiresContentUnloadedEvent()
        {
            bool eventFired = false;

            _textSprite.ContentUnloaded += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.UnloadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUnloadContentIsCalled_ThenContentUnloadingFiresBeforeContentUnloaded()
        {
            int callOrder = 0;
            int contentUnloadingOrder = 0;
            int contentUnloadedOrder = 0;

            _textSprite.ContentUnloading += delegate { contentUnloadingOrder = ++callOrder; };
            _textSprite.ContentUnloaded += delegate { contentUnloadedOrder = ++callOrder; };

            _textSprite.LoadContent();
            _textSprite.UnloadContent();

            Assert.That(contentUnloadingOrder, Is.LessThan(contentUnloadedOrder));
        }

        [Test]
        public void GivenNewTextSprite_WhenUpdateIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().Update(null));

        [Test]
        public void GivenLoadedTextSprite_WhenUpdateIsCalled_ThenFiresUpdatingEvent()
        {
            bool eventFired = false;

            _textSprite.Updating += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.Update(null);

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUpdateIsCalled_ThenFiresUpdatedEvent()
        {
            bool eventFired = false;

            _textSprite.Updated += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.Update(null);

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUpdateIsCalled_ThenUpdatingFiresBeforeUpdated()
        {
            int callOrder = 0;
            int updatingOrder = 0;
            int updatedOrder = 0;

            _textSprite.Updating += delegate { updatingOrder = ++callOrder; };
            _textSprite.Updated += delegate { updatedOrder = ++callOrder; };

            _textSprite.LoadContent();
            _textSprite.Update(null);

            Assert.That(updatingOrder, Is.LessThan(updatedOrder));
        }

        [Test]
        public void GivenNewTextSprite_WhenDrawIsCalled_ThenThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().Draw(null));

        [Test]
        public void GivenLoadedTextSprite_WhenDrawIsCalled_ThenFiresDrawingEvent()
        {
            bool eventFired = false;

            _textSprite.Drawing += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.Draw(null);

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDrawIsCalled_ThenFiresDrawnEvent()
        {
            bool eventFired = false;

            _textSprite.Drawn += delegate { eventFired = true; };

            _textSprite.LoadContent();
            _textSprite.Draw(null);

            Assert.That(eventFired);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDrawIsCalled_ThenDrawingFiresBeforeDrawn()
        {
            int callOrder = 0;
            int drawingOrder = 0;
            int drawnOrder = 0;

            _textSprite.Drawing += delegate { drawingOrder = ++callOrder; };
            _textSprite.Drawn += delegate { drawnOrder = ++callOrder; };

            _textSprite.LoadContent();
            _textSprite.Draw(null);

            Assert.That(drawingOrder, Is.LessThan(drawnOrder));
        }

        [Test]
        public void GivenLoadedTextSprite_WhenCheckingIsDisposed_ThenReturnsFalse()
        {
            _textSprite.LoadContent();

            Assert.That(_textSprite.IsDisposed, Is.False);
        }

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenTextIsEmpty()
            => Assert.That(new TextSprite().Text, Is.EqualTo(string.Empty));

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenIsContentLoadedIsFalse()
            => Assert.That(new TextSprite().IsContentLoaded, Is.False);

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenIsDisposedIsFalse()
            => Assert.That(new TextSprite().IsDisposed, Is.False);

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenIsActiveIsTrue()
            => Assert.That(new TextSprite().IsActive);

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenOpacityIsOne()
            => Assert.That(new TextSprite().Opacity, Is.EqualTo(1.0f));

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenTintIsWhite()
            => Assert.That(new TextSprite().Tint, Is.EqualTo(Colour.White));

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenOutlineColourIsBlack()
            => Assert.That(new TextSprite().OutlineColour, Is.EqualTo(Colour.Black));

        [Test]
        public void GivenNewTextSprite_WhenConstructed_ThenScaleIsOne()
            => Assert.That(new TextSprite().Scale, Is.EqualTo(Scale2D.One));

        [Test]
        public void GivenNewTextSprite_WhenLoadContentIsCalled_ThenIsContentLoadedIsTrue()
        {
            _textSprite.LoadContent();

            Assert.That(_textSprite.IsContentLoaded);
        }

        [Test]
        public void GivenNewTextSpriteWithEmptyText_WhenLoadContentIsCalled_ThenSpriteSizeIsOneByOne()
        {
            TextSprite textSprite = new() { Text = string.Empty };
            textSprite.LoadContent();

            Assert.That(textSprite.SpriteSize, Is.EqualTo(new Size2D(1, 1)));

            textSprite.Dispose();
        }

        [Test]
        public void GivenNewTextSpriteWithWhitespaceText_WhenLoadContentIsCalled_ThenTextIsEmpty()
        {
            TextSprite textSprite = new() { Text = "   " };
            textSprite.LoadContent();

            Assert.That(textSprite.Text, Is.EqualTo(string.Empty));

            textSprite.Dispose();
        }

        [Test]
        public void GivenNewTextSpriteWithPresetSpriteSize_WhenLoadContentIsCalled_ThenSpriteSizeIsPreserved()
        {
            Size2D expectedSize = new(100, 50);
            TextSprite textSprite = new() { SpriteSize = expectedSize };
            textSprite.LoadContent();

            Assert.That(textSprite.SpriteSize, Is.EqualTo(expectedSize));

            textSprite.Dispose();
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUnloadContentIsCalled_ThenIsContentLoadedIsFalse()
        {
            _textSprite.LoadContent();
            _textSprite.UnloadContent();

            Assert.That(_textSprite.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenLoadedTextSpriteWithText_WhenUnloadContentIsCalled_ThenTextIsEmpty()
        {
            TextSprite textSprite = new() { Text = "Hello", SpriteSize = new Size2D(100, 20) };
            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(textSprite.Text, Is.EqualTo(string.Empty));

            textSprite.Dispose();
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenIsDisposedIsTrue()
        {
            _textSprite.Dispose();

            Assert.That(_textSprite.IsDisposed);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDisposeIsCalled_ThenIsDisposedIsTrue()
        {
            _textSprite.LoadContent();
            _textSprite.Dispose();

            Assert.That(_textSprite.IsDisposed);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDisposeIsCalled_ThenIsContentLoadedIsFalse()
        {
            _textSprite.LoadContent();
            _textSprite.Dispose();

            Assert.That(_textSprite.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenDisposedTextSprite_WhenDisposeCalledAgain_ThenNoExceptionIsThrown()
        {
            _textSprite.Dispose();

            Assert.DoesNotThrow(_textSprite.Dispose);
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenFiresDisposingEvent()
        {
            bool eventFired = false;

            _textSprite.Disposing += delegate { eventFired = true; };
            _textSprite.Dispose();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenFiresDisposedEvent()
        {
            bool eventFired = false;

            _textSprite.Disposed += delegate { eventFired = true; };
            _textSprite.Dispose();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenTextSpriteWithLocationAndSize_WhenGettingClientRectangle_ThenMatchesLocationAndSize()
        {
            Point2D location = new(10, 20);
            Size2D size = new(100, 50);
            TextSprite textSprite = new() { Location = location, SpriteSize = size };
            textSprite.LoadContent();

            Assert.That(textSprite.ClientRectangle, Is.EqualTo(new Rectangle2D(location, size)));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithInactiveMovementEffect_WhenGettingClientRectangle_ThenMovementOffsetIsNotApplied()
        {
            Point2D location = new(10, 20);
            Size2D size = new(100, 50);
            TextSprite textSprite = new()
            {
                Location = location,
                SpriteSize = size,
                MovementEffect = new MovementEffect { TargetLocation = new Point2D(200, 200) }
            };
            textSprite.LoadContent();

            Assert.That(textSprite.ClientRectangle, Is.EqualTo(new Rectangle2D(location, size)));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithOpacity_WhenGettingClientOpacity_ThenReturnsBaseOpacity()
        {
            TextSprite textSprite = new() { Opacity = 0.5f };

            Assert.That(textSprite.ClientOpacity, Is.EqualTo(0.5f));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithInactiveOpacityEffect_WhenGettingClientOpacity_ThenReturnsBaseOpacity()
        {
            TextSprite textSprite = new()
            {
                Opacity = 0.8f,
                OpacityEffect = new FadeEffect { CurrentMultiplier = 0.5f }
            };
            textSprite.LoadContent();

            Assert.That(textSprite.ClientOpacity, Is.EqualTo(0.8f));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithActiveOpacityEffect_WhenGettingClientOpacity_ThenReturnsMultipliedOpacity()
        {
            TextSprite textSprite = new()
            {
                Opacity = 1.0f,
                OpacityEffect = new FadeEffect { CurrentMultiplier = 0.5f }
            };
            textSprite.LoadContent();
            textSprite.OpacityEffect.Activate();

            Assert.That(textSprite.ClientOpacity, Is.EqualTo(0.5f).Within(0.001f));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithRotation_WhenGettingClientRotation_ThenReturnsBaseRotation()
        {
            TextSprite textSprite = new() { Rotation = 1.5f };

            Assert.That(textSprite.ClientRotation, Is.EqualTo(1.5f));

            textSprite.Dispose();
        }

        [Test]
        public void GivenTextSpriteWithNoScaleEffect_WhenGettingClientScale_ThenReturnsBaseScale()
        {
            Assert.That(_textSprite.ClientScale, Is.EqualTo(Scale2D.One));
        }

        [Test]
        public void GivenTextSpriteWithActiveZoomEffect_WhenGettingClientScale_ThenReturnsScaledValue()
        {
            TextSprite textSprite = new()
            {
                Scale = Scale2D.One,
                ScaleEffect = new ZoomEffect { CurrentHorizontalMultiplier = 2.0f, CurrentVerticalMultiplier = 2.0f }
            };
            textSprite.LoadContent();
            textSprite.ScaleEffect.Activate();

            Assert.That(textSprite.ClientScale, Is.EqualTo(new Scale2D(2.0f, 2.0f)));

            textSprite.Dispose();
        }
    }
}
