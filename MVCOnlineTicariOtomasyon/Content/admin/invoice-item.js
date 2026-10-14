/* Ticari Otomasyon - fatura kalemi formu */
(function () {
  'use strict';

  function update(form) {
    if (!form) { return; }
    var quantity = form.querySelector('[name=InvoiceItemQuantity]');
    var price = form.querySelector('[name=InvoiceItemUnitPrice]');
    var total = form.querySelector('[data-item-total]');
    if (!quantity || !price || !total) { return; }

    // Ondalık ayracı sunucunun bölge ayarına göre
    var separator = total.getAttribute('data-decimal') || ',';
    var pattern = separator === ',' ? /^\d+(,\d+)?$/ : /^\d+(\.\d+)?$/;
    var text = price.value.trim();
    var unit = pattern.test(text) ? Number(text.replace(',', '.')) : NaN;
    var count = Number(quantity.value);

    total.textContent = count > 0 && unit > 0
      ? (count * unit).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })
      : '–';
  }

  function updateAll() {
    Array.prototype.forEach.call(document.querySelectorAll('[data-item-total]'), function (total) {
      update(total.closest('form'));
    });
  }

  document.addEventListener('input', function (e) { update(e.target.form); });

  // Pencereye sunucudan yeni form geldiğinde tutarı yeniden yaz
  document.addEventListener('modal-form:placed', updateAll);

  updateAll();
})();
