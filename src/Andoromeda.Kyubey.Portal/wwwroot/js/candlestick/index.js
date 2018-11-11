

var kyubeyExchangeCandlestick = (function () {
    var defaultActiveCycle = 240;
    //var tokenId = window.tokenId;

    var addCandlestickClass = function (fatherDom, dom) {
        [...fatherDom.getElementsByTagName('span')].forEach(function (item) {
            item.className = ''
        })
        dom.className = 'active'
    }
    var addInternalChangedEventListener = function () {
        var _this = this;
        var intervalDom = document.getElementById('interval')
        // 切换产品周期
        intervalDom.addEventListener('click', function (e) {
            // e.target.dataset.value 这个就是获取的产品的周期
            widget.chart().setResolution(e.target.dataset.value)


            // 这个函数不用看，我为了样式好看 写一个添加删除class
            addCandlestickClass(intervalDom, e.target)
        }, false)
    }
    var initCandlestick = function () {
        var _this = this;
        window.TradingView.onready(function () {
            // chartConfig 在chartConfig.js里面
            // 给chartConfig添加展示周期
            chartConfig.interval =defaultActiveCycle;
            // 给chartConfig添加展示产品
            chartConfig.symbol = `${window.tokenId}/EOS`// index_market

            // 初始化 TradingView
            widget = new window.TradingView.widget(chartConfig)

            widget && widget.onChartReady && widget.onChartReady(function () {
                //// 这是k线图 展示的 7日均线和30日均线。
                //widget.chart().createStudy('Moving Average', false, false, [7], null, {'Plot.linewidth': 2, 'Plot.color': '#2ba7d6'})
                //widget.chart().createStudy('Moving Average', false, false, [30], null, {'Plot.linewidth': 2, 'Plot.color': '#de9f66'})
            })
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
            debugger;
            var perioid = getResolutionSecounds(resolution);

            ajaxCallBack(window.tokenId, new Date(rangeStartDate * 1000), new Date(rangeEndDate * 1000), perioid, function (data) {
                if (data && Array.isArray(data)) {
                    //debugger;
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
        debugger;
        window.tokenId = tokenId;
        initCandlestick();
        bindCandlestick(ajaxCallback);
        addInternalChangedEventListener();
    }
    return {
        init: init
    }
})();