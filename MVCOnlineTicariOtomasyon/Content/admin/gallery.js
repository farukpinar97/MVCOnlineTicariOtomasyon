/* Ticari Otomasyon - galeri */
(function () {
  'use strict';

  var box = document.getElementById('lightbox');
  if (!box || !window.jQuery) { return; }

  var $box = window.jQuery(box);
  var image = box.querySelector('.lightbox__image');
  var title = box.querySelector('.lightbox__title');
  var meta = box.querySelector('.lightbox__meta');
  var count = box.querySelector('.lightbox__count');
  var prev = box.querySelector('.lightbox__prev');
  var next = box.querySelector('.lightbox__next');

  var items = [];   // pencere açıldığı anda gezilebilecek karolar
  var index = 0;   // şu an gösterilen karonun sırası

  /* Yüklenemeyen görseller */
  function markBroken(img) {
    var item = img.closest('.gallery__item');
    if (!item || item.classList.contains('is-broken')) { return; }

    item.classList.add('is-broken');
    item.disabled = true;

    var note = document.createElement('span');
    note.className = 'gallery__broken';
    note.textContent = 'Görsel yüklenemedi';
    img.parentNode.replaceChild(note, img);
  }

  // "error" olayı yukarı taşmadığı için yakalama aşamasında dinlenir
  document.addEventListener('error', function (e) {
    var el = e.target;
    if (el && el.tagName === 'IMG' && el.closest('.gallery__item')) { markBroken(el); }
  }, true);

  // Bu dosya çalışmadan önce yüklenmeyi deneyip başaramamış görseller
  Array.prototype.forEach.call(document.querySelectorAll('.gallery__item img'), function (img) {
    if (img.complete && img.currentSrc && img.naturalWidth === 0) { markBroken(img); }
  });

  /* Büyük görsel */
  function preload(item) {
    if (item) { new Image().src = item.getAttribute('data-full'); }
  }

  function show(newIndex) {
    index = (newIndex + items.length) % items.length;   // sondan sonra başa, baştan önce sona
    var item = items[index];

    image.src = item.getAttribute('data-full');
    image.alt = item.getAttribute('data-title');
    title.textContent = item.getAttribute('data-title');
    meta.textContent = item.getAttribute('data-meta');
    count.textContent = (index + 1) + ' / ' + items.length;

    // Geçiş beklemesin diye komşu görseller önceden yüklenir
    if (items.length > 1) {
      preload(items[(index + 1) % items.length]);
      preload(items[(index - 1 + items.length) % items.length]);
    }
  }

  function step(direction) {
    if (items.length > 1) { show(index + direction); }
  }

  document.addEventListener('click', function (e) {
    var item = e.target.closest ? e.target.closest('.gallery__item') : null;
    if (!item || item.disabled) { return; }

    // Aramayla gizlenmiş ve yüklenememiş karolar gezintiye girmez
    items = Array.prototype.slice.call(document.querySelectorAll('.gallery__item:not([hidden]):not(.is-broken)'));
    var start = items.indexOf(item);
    if (start === -1) { return; }

    prev.hidden = next.hidden = items.length < 2;
    show(start);
    $box.modal('show');
  });

  prev.addEventListener('click', function () { step(-1); });
  next.addEventListener('click', function () { step(1); });

  document.addEventListener('keydown', function (e) {
    if (!box.classList.contains('show')) { return; }
    if (e.key === 'ArrowLeft') { e.preventDefault(); step(-1); }
    if (e.key === 'ArrowRight') { e.preventDefault(); step(1); }
  });

  // Dokunmatik
  var touchX = null;
  var touchY = null;

  box.addEventListener('touchstart', function (e) {
    touchX = e.changedTouches[0].clientX;
    touchY = e.changedTouches[0].clientY;
  }, { passive: true });

  box.addEventListener('touchend', function (e) {
    if (touchX === null) { return; }
    var dx = e.changedTouches[0].clientX - touchX;
    var dy = e.changedTouches[0].clientY - touchY;
    touchX = null;

    // En az 40 piksel ve yataydan çok dikey olmayan hareket kaydırma sayılır
    if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) { step(dx < 0 ? 1 : -1); }
  }, { passive: true });

  // Kapanınca
  $box.on('hidden.bs.modal', function () {
    image.removeAttribute('src');
    if (items[index]) { items[index].focus(); }
  });
})();
