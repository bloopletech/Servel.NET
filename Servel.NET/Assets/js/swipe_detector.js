"use strict";
// Based on https://github.com/john-doherty/swiped-events/blob/dd3bddcfdb744ff2b31d9152ac482966ebb538ab/src/swiped-events.js
const SwipeDetector = (function () {
  // Based on https://stackoverflow.com/a/9614122
  function angleDeg(dx, dy) {
    let theta = Math.atan2(dy, dx) * (180 / Math.PI);
    return theta < 0 ? 360 + theta : theta;
  }

  function angleDirection(angle, maxSlopeDeg) {
    if(angle >= 360 - maxSlopeDeg || angle <= maxSlopeDeg) return "right";
    if(angle >= 90 - maxSlopeDeg && angle <= 90 + maxSlopeDeg) return "down";
    if(angle >= 180 - maxSlopeDeg && angle <= 180 + maxSlopeDeg) return "left";
    if(angle >= 270 - maxSlopeDeg && angle <= 270 + maxSlopeDeg) return "up";
    return null;
  }

  const Mouse = function (handler, options) {
    const { minLengthPx, maxSlopeDeg, timeoutMs } = { ...{ minLengthPx: 100, maxSlopeDeg: 20, timeoutMs: 500 }, options };

    function onEnd(e, startX, startY) {
      const endX = e.clientX;
      const endY = e.clientY;
      const diffX = endX - startX;
      const diffY = endY - startY;

      if(Math.hypot(diffX, diffY) < minLengthPx) return;

      const direction = angleDirection(angleDeg(diffX, diffY), maxSlopeDeg);
      if(!direction) return;

      handler({
        dir: direction,
        target: e.target,
        pointerType: "mouse",
        startX: Math.round(startX),
        startY: Math.round(startY),
        endX: Math.round(endX),
        endY: Math.round(endY)
      });
    }

    function onStart(e) {
      if(e.buttons != 1) return;

      const startX = e.clientX;
      const startY = e.clientY;

      const aborter = new AbortController();
      addEventListener("mouseup", (e) => { onEnd(e, startX, startY); aborter.abort(); }, { signal: aborter.signal });
      setTimeout(() => aborter.abort(), timeoutMs);
    }

    addEventListener("mousedown", onStart);
  };

  const Touch = function (handler, options) {
    const { minLengthPx, maxSlopeDeg, timeoutMs } = { ...{ minLengthPx: 75, maxSlopeDeg: 20, timeoutMs: 500 }, options };

    function onEnd(e, startX, startY) {
      const changedTouches = e.changedTouches || e.touches;
      if(changedTouches.length > 1) return;

      const endX = changedTouches[0].clientX;
      const endY = changedTouches[0].clientY;
      const diffX = endX - startX;
      const diffY = endY - startY;

      if(Math.hypot(diffX, diffY) < minLengthPx) return;

      const direction = angleDirection(angleDeg(diffX, diffY), maxSlopeDeg);
      if(!direction) return;

      handler({
        dir: direction,
        target: e.target,
        pointerType: changedTouches[0].touchType,
        startX: Math.round(startX),
        startY: Math.round(startY),
        endX: Math.round(endX),
        endY: Math.round(endY)
      });
    }

    function onStart(e) {
      if(e.touches.length > 1) return;

      const startX = e.touches[0].clientX;
      const startY = e.touches[0].clientY;

      const aborter = new AbortController();
      addEventListener("touchend", (e) => { onEnd(e, startX, startY); aborter.abort(); }, { signal: aborter.signal });
      addEventListener("touchcancel", (e) => aborter.abort(), { signal: aborter.signal });
      setTimeout(() => aborter.abort(), timeoutMs);
    }

    addEventListener("touchstart", onStart);
  };

  return {
    Mouse: Mouse,
    Touch: Touch
  }
})();
