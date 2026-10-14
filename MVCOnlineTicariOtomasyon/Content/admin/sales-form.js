/* Ticari Otomasyon - satış formu */
(function () {
  'use strict';

  var box = document.querySelector('[data-sale-form]');
  if (!box) { return; }

  var product = box.querySelector('#ProductId');
  var quantity = box.querySelector('#SalesTransactionQuantity');
  var price = box.querySelector('#SalesTransactionUnitPrice');
  var total = box.querySelector('#saleTotal');
  var hint = box.querySelector('#stockHint');

  // Sunucunun ondalık ayracı
  var separator = box.getAttribute('data-decimal') || ',';

  function selectedOption() {
    var option = product.options[product.selectedIndex];
    return option && option.value ? option : null;
  }

  function readPrice() {
    var text = price.value.trim();
    var pattern = separator === ',' ? /^\d+(,\d+)?$/ : /^\d+(\.\d+)?$/;
    return pattern.test(text) ? Number(text.replace(',', '.')) : NaN;
  }

  function writePrice(value) {
    price.value = value.toFixed(2).replace('.', separator);
  }

  function update() {
    var option = selectedOption();
    var count = Number(quantity.value);
    var unit = readPrice();

    if (option) {
      var stock = Number(option.getAttribute('data-stock'));
      var tooMany = count > stock;
      quantity.max = stock;
      hint.textContent = tooMany ? 'Stokta yalnızca ' + stock + ' adet var.' : 'Stokta ' + stock + ' adet var.';
      hint.classList.toggle('text-danger', tooMany);
      hint.classList.toggle('text-muted', !tooMany);
    } else {
      quantity.removeAttribute('max');
      hint.textContent = '';
    }

    total.textContent = count > 0 && unit > 0
      ? (count * unit).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })
      : '\u2013';
  }

  // Ürün değişince fiyatı o ürünün satış fiyatı yap
  product.addEventListener('change', function () {
    var option = selectedOption();
    if (option) { writePrice(Number(option.getAttribute('data-price'))); }
    update();
  });

  quantity.addEventListener('input', update);
  price.addEventListener('input', update);

  // Açılış
  var initial = selectedOption();
  if (initial && !price.value.trim()) { writePrice(Number(initial.getAttribute('data-price'))); }
  update();
})();
