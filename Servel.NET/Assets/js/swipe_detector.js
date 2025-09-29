"use strict";
// Based on https://github.com/john-doherty/swiped-events/blob/dd3bddcfdb744ff2b31d9152ac482966ebb538ab/src/swiped-events.js
const SwipeDetector = function (element, handler, options) {
  const { minLengthPx, maxSlopeDeg, timeoutMs } = { ...{ minLengthPx: 75, maxSlopeDeg: 20, timeoutMs: 500 }, options };

  // Based on https://stackoverflow.com/a/9614122
  function angleDeg(dx, dy) {
    let theta = Math.atan2(dy, dx) * (180 / Math.PI);
    return theta < 0 ? 360 + theta : theta;
  }

  function angleDirection(angle) {
    if(angle >= 360 - maxSlopeDeg || angle <= maxSlopeDeg) return "right";
    if(angle >= 90 - maxSlopeDeg && angle <= 90 + maxSlopeDeg) return "down";
    if(angle >= 180 - maxSlopeDeg && angle <= 180 + maxSlopeDeg) return "left";
    if(angle >= 270 - maxSlopeDeg && angle <= 270 + maxSlopeDeg) return "up";
    return null;
  }

  function onEnd(e, startX, startY) {
    if(!e.isPrimary) return;

    const endX = e.clientX;
    const endY = e.clientY;
    const diffX = endX - startX;
    const diffY = endY - startY;

    if(Math.hypot(diffX, diffY) < minLengthPx) return;

    const direction = angleDirection(angleDeg(diffX, diffY));
    if(!direction) return;

    handler({
      dir: direction,
      pointerType: e.pointerType,
      startX: Math.round(startX),
      startY: Math.round(startY),
      endX: Math.round(endX),
      endY: Math.round(endY)
    });
  }

  function onStart(e) {
    if(!e.isPrimary) return;

    const startX = e.clientX;
    const startY = e.clientY;

    const aborter = new AbortController();
    addEventListener("pointerup", (e) => { onEnd(e, startX, startY); aborter.abort(); }, { signal: aborter.signal });
    addEventListener("pointercancel", () => aborter.abort(), { signal: aborter.signal });
    setTimeout(() => aborter.abort(), timeoutMs);
  }

  element.addEventListener("pointerdown", onStart);
};
