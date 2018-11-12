module.exports = {
  buyPrice: function (callback, rows, funcStr) {
    eval(funcStr);
    try {
      var price = getCurrentBuyPrice(rows);
      callback(null, price);
    }
    catch (err) {
      callback(err);
    }
  },
  sellPrice: function (callback, rows, funcStr) {
    eval(funcStr);
    try {
      var price = getCurrentSellPrice(rows);
      callback(null, price);
    }
    catch (err) {
      callback(err);
    }
  }
};