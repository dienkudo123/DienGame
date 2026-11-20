mergeInto(LibraryManager.library, {
  DienGame_ConfigureMobileWeb: function () {
    try {
      var meta = document.querySelector('meta[name="viewport"]');
      if (!meta) {
        meta = document.createElement('meta');
        meta.name = 'viewport';
        document.head.appendChild(meta);
      }
      meta.content = 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, viewport-fit=cover';

      document.documentElement.style.overscrollBehavior = 'none';
      document.body.style.overscrollBehavior = 'none';
      document.body.style.margin = '0';
      document.body.style.touchAction = 'none';

      var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
      if (canvas) {
        canvas.style.touchAction = 'none';
        canvas.style.webkitUserSelect = 'none';
        canvas.style.userSelect = 'none';
        canvas.addEventListener('contextmenu', function (event) { event.preventDefault(); });
        canvas.addEventListener('touchmove', function (event) { event.preventDefault(); }, { passive: false });
      }

      document.addEventListener('gesturestart', function (event) { event.preventDefault(); }, { passive: false });
      document.addEventListener('gesturechange', function (event) { event.preventDefault(); }, { passive: false });
    } catch (error) {
      console.warn('DienGame mobile web configuration failed', error);
    }
  }
});
