using System;

using Microsoft.Xna.Framework;
using NUnit.Framework;

using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

namespace NuciXNA.Graphics.UnitTests.Drawing
{
    public class TextSpriteTests
    {
        [Test]
        public void LoadContent_ContentAlreadyLoaded_ThrowsInvalidOperationException()
        {
            TextSprite textSprite = new();
            textSprite.LoadContent();

            Assert.Throws<InvalidOperationException>(() => textSprite.LoadContent());
        }

        [Test]
        public void LoadContent_FiresContentLoading()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.ContentLoading += delegate { eventFired = true; };

            textSprite.LoadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void LoadContent_FiresContentLoaded()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.ContentLoaded += delegate { eventFired = true; };

            textSprite.LoadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void LoadContent_FiresContentLoadingBeforeContentLoaded()
        {
            DateTime firstEventTime = DateTime.Now;
            DateTime lastEventTime = DateTime.Now;

            TextSprite textSprite = new();
            textSprite.ContentLoading += delegate { firstEventTime = DateTime.Now; };
            textSprite.ContentLoading += delegate { lastEventTime = DateTime.Now; };

            textSprite.LoadContent();

            Assert.That(firstEventTime < lastEventTime);
        }

        [Test]
        public void UnloadContent_ContentNotLoaded_ThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().UnloadContent());

        [Test]
        public void UnloadContent_FiresContentUnloading()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.ContentUnloading += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void UnloadContent_FiresContentUnloaded()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.ContentUnloaded += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(eventFired);
        }

        [Test]
        public void UnloadContent_FiresContentUnloadingBeforeContentUnloaded()
        {
            DateTime firstEventTime = DateTime.Now;
            DateTime lastEventTime = DateTime.Now;

            TextSprite textSprite = new();
            textSprite.ContentUnloading += delegate { firstEventTime = DateTime.Now; };
            textSprite.ContentUnloading += delegate { lastEventTime = DateTime.Now; };

            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(firstEventTime < lastEventTime);
        }

        [Test]
        public void Update_ContentNotLoaded_ThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().Update(null));

        [Test]
        public void Update_FiresUpdating()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.Updating += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.Update(null);

            Assert.That(eventFired);
        }

        [Test]
        public void Update_FiresUpdated()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.Updated += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.Update(null);

            Assert.That(eventFired);
        }

        [Test]
        public void Update_FiresUpdatingBeforeUpdated()
        {
            DateTime firstEventTime = DateTime.Now;
            DateTime lastEventTime = DateTime.Now;

            TextSprite textSprite = new();
            textSprite.Updating += delegate { firstEventTime = DateTime.Now; };
            textSprite.Updating += delegate { lastEventTime = DateTime.Now; };

            textSprite.LoadContent();
            textSprite.Update(null);

            Assert.That(firstEventTime < lastEventTime);
        }

        [Test]
        public void Draw_ContentNotLoaded_ThrowsInvalidOperationException()
            => Assert.Throws<InvalidOperationException>(() => new TextSprite().Draw(null));

        [Test]
        public void Draw_FiresDrawing()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.Drawing += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.Draw(null);

            Assert.That(eventFired);
        }

        [Test]
        public void Draw_FiresDrawn()
        {
            bool eventFired = false;

            TextSprite textSprite = new();
            textSprite.Drawn += delegate { eventFired = true; };

            textSprite.LoadContent();
            textSprite.Draw(null);

            Assert.That(eventFired);
        }

        [Test]
        public void Draw_FiresDrawingBeforeDrawn()
        {
            DateTime firstEventTime = DateTime.Now;
            DateTime lastEventTime = DateTime.Now;

            TextSprite textSprite = new();
            textSprite.Drawing += delegate { firstEventTime = DateTime.Now; };
            textSprite.Drawing += delegate { lastEventTime = DateTime.Now; };

            textSprite.LoadContent();
            textSprite.Draw(null);

            Assert.That(firstEventTime < lastEventTime);
        }

        [Test]
        public void GetIsDisposed_NotDisposed_ReturnsFalse()
        {
            TextSprite textSprite = new();
            textSprite.LoadContent();

            Assert.That(textSprite.IsDisposed, Is.False);
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
            => Assert.That(new TextSprite().IsActive, Is.True);

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
            TextSprite textSprite = new();
            textSprite.LoadContent();

            Assert.That(textSprite.IsContentLoaded, Is.True);
        }

        [Test]
        public void GivenNewTextSpriteWithEmptyText_WhenLoadContentIsCalled_ThenSpriteSizeIsOneByOne()
        {
            TextSprite textSprite = new() { Text = string.Empty };
            textSprite.LoadContent();

            Assert.That(textSprite.SpriteSize, Is.EqualTo(new Size2D(1, 1)));
        }

        [Test]
        public void GivenNewTextSpriteWithWhitespaceText_WhenLoadContentIsCalled_ThenTextIsEmpty()
        {
            TextSprite textSprite = new() { Text = "   " };
            textSprite.LoadContent();

            Assert.That(textSprite.Text, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GivenNewTextSpriteWithPresetSpriteSize_WhenLoadContentIsCalled_ThenSpriteSizeIsPreserved()
        {
            Size2D expectedSize = new(100, 50);
            TextSprite textSprite = new() { SpriteSize = expectedSize };
            textSprite.LoadContent();

            Assert.That(textSprite.SpriteSize, Is.EqualTo(expectedSize));
        }

        [Test]
        public void GivenLoadedTextSprite_WhenUnloadContentIsCalled_ThenIsContentLoadedIsFalse()
        {
            TextSprite textSprite = new();
            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(textSprite.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenLoadedTextSpriteWithText_WhenUnloadContentIsCalled_ThenTextIsEmpty()
        {
            TextSprite textSprite = new() { Text = "Hello", SpriteSize = new Size2D(100, 20) };
            textSprite.LoadContent();
            textSprite.UnloadContent();

            Assert.That(textSprite.Text, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenIsDisposedIsTrue()
        {
            TextSprite textSprite = new();
            textSprite.Dispose();

            Assert.That(textSprite.IsDisposed, Is.True);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDisposeIsCalled_ThenIsDisposedIsTrue()
        {
            TextSprite textSprite = new();
            textSprite.LoadContent();
            textSprite.Dispose();

            Assert.That(textSprite.IsDisposed, Is.True);
        }

        [Test]
        public void GivenLoadedTextSprite_WhenDisposeIsCalled_ThenIsContentLoadedIsFalse()
        {
            TextSprite textSprite = new();
            textSprite.LoadContent();
            textSprite.Dispose();

            Assert.That(textSprite.IsContentLoaded, Is.False);
        }

        [Test]
        public void GivenDisposedTextSprite_WhenDisposeCalledAgain_ThenNoExceptionIsThrown()
        {
            TextSprite textSprite = new();
            textSprite.Dispose();

            Assert.DoesNotThrow(() => textSprite.Dispose());
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenFiresDisposingEvent()
        {
            bool eventFired = false;
            TextSprite textSprite = new();
            textSprite.Disposing += delegate { eventFired = true; };
            textSprite.Dispose();

            Assert.That(eventFired);
        }

        [Test]
        public void GivenNewTextSprite_WhenDisposeIsCalled_ThenFiresDisposedEvent()
        {
            bool eventFired = false;
            TextSprite textSprite = new();
            textSprite.Disposed += delegate { eventFired = true; };
            textSprite.Dispose();

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
        }

        [Test]
        public void GivenTextSpriteWithNoScaleEffect_WhenGettingClientScale_ThenReturnsBaseScale()
        {
            TextSprite textSprite = new();

            Assert.That(textSprite.ClientScale, Is.EqualTo(Scale2D.One));
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
