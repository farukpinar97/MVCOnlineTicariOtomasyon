/* Ticari Otomasyon - pencere formları */
(function () {
  'use strict';

  if (!window.fetch || !window.jQuery) { return; }   // betik çalışamazsa formlar normal şekilde gönderilir

  // Sunucuya "bu bir AJAX isteği" demek için
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
    slot.setAttribute('data-changed', 'true');
    focusFirst(slot);

    // Yeni gelen formla ilgilenen başka betikler için haber ver
    slot.dispatchEvent(new CustomEvent('modal-form:placed', { bubbles: true }));
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

    // Pencere kapanınca içine boş form gelsin
    $modal.on('hidden.bs.modal', function () {
      var done = slot.querySelector('[data-register-done]');
      var changed = slot.hasAttribute('data-reset') && slot.getAttribute('data-changed') === 'true';
      if (!done && !changed) { return; }
      slot.removeAttribute('data-changed');
      fetch(slot.getAttribute('data-url'), { headers: ajaxHeaders, credentials: 'same-origin' })
        .then(function (response) { return response.ok ? response.text() : null; })
        .then(function (html) {
          if (!html) { return; }
          slot.innerHTML = html;
          slot.dispatchEvent(new CustomEvent('modal-form:placed', { bubbles: true }));
        });
    });
  });

  // Giriş isteyen bir sayfadan gelindiyse ilgili pencere kendiliğinden açılır
  var autoOpen = document.body.getAttribute('data-open');
  if (autoOpen) { window.jQuery(autoOpen).modal('show'); }
})();
