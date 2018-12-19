

var kyubeyExchangeCandlestick = (function () {
    var defaultActiveCycle = 240;

    var addCandlestickClass = function (fatherDom, dom) {
        [...fatherDom.getElementsByTagName('span')].forEach(function (item) {
            item.className = ''
        })
        dom.className = 'active'
    }
    var addInternalChangedEventListener = function () {
        var _this = this;
        var intervalDom = document.getElementById('interval')
        intervalDom.addEventListener('click', function (e) {
            // e.target.dataset.value   it is get product life cycle
            widget.chart().setResolution(e.target.dataset.value)
            addCandlestickClass(intervalDom, e.target)
        }, false)
    }
    var initCandlestick = function () {
        var _this = this;
        window.TradingView.onready(function () {
            chartConfig.interval = defaultActiveCycle;
            chartConfig.symbol = `${window.tokenId}/EOS`// index_market
            widget = new window.TradingView.widget(chartConfig)
        })
    }
    var getResolutionSecounds = function (resolution) {
        var perioid = 60;
        switch (resolution) {
            case '1D':
                perioid = 24 * 60 * 60;
                break;
            default:
                perioid = parseInt(resolution) * 60;
        }
        return perioid;
    }
    var bindCandlestick = function (ajaxCallBack) {
        var _this = this;
        FeedBase.prototype.getBars = function (symbolInfo, resolution, rangeStartDate, rangeEndDate, onResult, onError) {
            var perioid = getResolutionSecounds(resolution);

            ajaxCallBack(window.tokenId, new Date(rangeStartDate * 1000), new Date(rangeEndDate * 1000), perioid, function (data) {
                if (data && Array.isArray(data)) {
                    var meta = { noData: false }
                    var bars = []
                    if (data.length) {
                        for (var i = 0; i < data.length; i += 1) {
                            bars.push({
                                time: Number(new Date(data[i].time)),
                                close: data[i].closing,
                                open: data[i].opening,
                                high: data[i].max,
                                low: data[i].min,
                                volume: data[i].volume
                            })
                        }
                    } else {
                        meta = { noData: true }
                    }
                    onResult(bars, meta)
                }
            });
        }
    }
    function init(tokenId, ajaxCallback) {
        window.tokenId = tokenId;
        initCandlestick();
        bindCandlestick(ajaxCallback);
        addInternalChangedEventListener();
    }
    function local() {
        var currentLang = getCookie("ASPNET_LANG");
        if (currentLang == "ja" )
            return currentLang;
        if (currentLang == "zh-CN" || currentLang == "")
            return "zh";
        if (currentLang == "zh-TW")
            return "zh_TW";
        if (currentLang == "en-US")
            return "en";
    }
    return {
        init: init,
        local: local
    }
})();