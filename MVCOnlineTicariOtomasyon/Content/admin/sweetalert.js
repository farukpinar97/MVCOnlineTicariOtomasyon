/* Ticari Otomasyon - SweetAlert2 örneği */
(function () {
  'use strict';

  if (!window.Swal) { return; }   // kütüphane yüklenemediyse butonlar hiçbir şey yapmaz

  // Tema renkleri
  var COLORS = { success: '#3B7F2E', error: '#B93A2B', warning: '#E2A52B' };

  var dialog = window.Swal.mixin({
    buttonsStyling: false,
    reverseButtons: true,   // "Vazgeç" solda, asıl işlem sağda
    focusCancel: true,   // Enter'a yanlışlıkla basınca silinmesin
    customClass: {
      confirmButton: 'btn btn-danger',
      cancelButton: 'btn btn-light'
    }
  });

  /* 1. Silme onayı */
  function postTo(url) {
    var form = document.createElement('form');
    form.method = 'post';
    form.action = url;

    // [ValidateAntiForgeryToken] için
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) { form.appendChild(token.cloneNode()); }

    document.body.appendChild(form);
    form.submit();
  }

  document.addEventListener('click', function (e) {
    var button = e.target.closest('[data-swal-delete]');
    if (!button) { return; }
    e.preventDefault();

    var name = button.getAttribute('data-delete-name');

    dialog.fire({
      icon: 'warning',
      iconColor: COLORS.warning,
      title: name ? '“' + name + '” silinsin mi?' : 'Kayıt silinsin mi?',
      text: button.getAttribute('data-delete-text') || 'Bu işlem geri alınamaz.',
      showCancelButton: true,
      confirmButtonText: 'Evet, sil',
      cancelButtonText: 'Vazgeç'
    }).then(function (result) {
      if (!result.isConfirmed) { return; }

      // Gönderilirken ikinci kez tıklanamasın, beklediği görünsün
      window.Swal.fire({
        title: 'Siliniyor…',
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: function () { window.Swal.showLoading(); }
      });
      postTo(button.getAttribute('data-delete-url'));
    });
  });

  /* 2. Bildirim */
  var flash = document.querySelector('[data-swal-flash]');
  if (flash) {
    var type = flash.getAttribute('data-swal-flash') === 'error' ? 'error' : 'success';

    window.Swal.fire({
      toast: true,
      position: 'top-end',
      icon: type,
      iconColor: COLORS[type],
      title: flash.textContent.trim(),
      showConfirmButton: false,
      showCloseButton: true,
      timer: type === 'error' ? 6000 : 3500,   // hata mesajı daha uzun kalsın
      timerProgressBar: true
    });
  }
})();
