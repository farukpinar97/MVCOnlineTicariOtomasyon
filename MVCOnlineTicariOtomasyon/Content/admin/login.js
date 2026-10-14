/* ==========================================================================
   Ticari Otomasyon - giriş sayfası (Views/Login/Index.cshtml)
   Penceredeki formları (cari girişi, yönetici girişi, kayıt) sayfa yenilenmeden gönderir.
   Sunucunun döndürdüğü partial pencerenin içine yerleşir:
   - hatalı form ya da "kaydın oluşturuldu": olduğu gibi gösterilir
   - <div data-redirect="adres">: giriş başarılı, sayfa o adrese gider
   Pencere içi:  <div class="modal-content" data-modal-slot data-url="formun boş halinin adresi">
   ========================================================================== */
(function () {
  'use strict';

  if (!window.fetch || !window.jQuery) { return; }   // betik çalışamazsa formlar normal şekilde gönderilir

  // Sunucuya "bu bir AJAX isteği" demek için: controller'daki Request.IsAjaxRequest() buna bakar
  var ajaxHeaders = { 'X-Requested-With': 'XMLHttpRequest' };

  function focusFirst(slot) {
    var target = slot.querySelector('.input-validation-error') || slot.querySelector('input:not([type=hidden])');
    if (target) { target.focus(); }
  }

  function place(slot, html) {
    var box = document.createElement('div');
    box.innerHTML = html;

    var redirect = box.querySelector('[data-redirect]');
    if (redirect) {
      window.location.href = redirect.getAttribute('data-redirect');
      return;
    }

    slot.innerHTML = html;
    focusFirst(slot);
  }

  Array.prototype.forEach.call(document.querySelectorAll('[data-modal-slot]'), function (slot) {
    var $modal = window.jQuery(slot).closest('.modal');

    slot.addEventListener('submit', function (e) {
      var form = e.target;
      if (!form || form.tagName !== 'FORM') { return; }
      e.preventDefault();

      var button = form.querySelector('[type=submit]');
      var error = form.querySelector('.modal-form__error');
      button.disabled = true;
      if (error) { error.hidden = true; }

      fetch(form.action, { method: 'POST', body: new FormData(form), headers: ajaxHeaders, credentials: 'same-origin' })
        .then(function (response) {
          if (!response.ok) { throw new Error(response.status); }
          return response.text();
        })
        .then(function (html) { place(slot, html); })
        .catch(function () {
          button.disabled = false;
          if (error) { error.hidden = false; }
        });
    });

    $modal.on('shown.bs.modal', function () { focusFirst(slot); });

    // Kayıt tamamlandıysa pencere kapanınca içine boş form gelsin (bir sonraki kayıt için)
    $modal.on('hidden.bs.modal', function () {
      if (!slot.querySelector('[data-register-done]')) { return; }
      fetch(slot.getAttribute('data-url'), { headers: ajaxHeaders, credentials: 'same-origin' })
        .then(function (response) { return response.ok ? response.text() : null; })
        .then(function (html) { if (html) { slot.innerHTML = html; } });
    });
  });

  // Giriş isteyen bir sayfadan gelindiyse (adreste ReturnUrl var) ilgili pencere kendiliğinden açılır
  var autoOpen = document.body.getAttribute('data-open');
  if (autoOpen) { window.jQuery(autoOpen).modal('show'); }
})();
