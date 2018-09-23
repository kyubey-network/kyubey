var chain_id = 'aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906';
var host = 'nodes.get-scatter.com';
var account, eos, requiredFields;

function showModal(title, content) {
    $('#modalTitle').text(title);
    $('#modalContent').text(content);
    $('#modal').modal('show');
}

setTimeout(function () {
    if (!('scatter' in window)) {
        showModal('Scatter插件没有找到', 'Scatter是一款EOS钱包，运行在Chrome浏览器中，请您确定已经安装Scatter插件. 参考：https://www.jianshu.com/p/a2e1e6204023');
    } else {
        var network = {
            blockchain: 'eos',
            host: host,
            port: 443,
            protocol: 'https',
            chainId: chain_id
        };

        scatter.getIdentity({ accounts: [network] }).then(identity => {
            account = identity.accounts.find(acc => acc.blockchain === 'eos');
            eos = scatter.eos(network, Eos, {});
            requiredFields = { accounts: [network] };
        });
    }
}, 2000);

setInterval(function () {
    if (eos && account) {
        eos.getCurrencyBalance('eosio.token', account.name).then(x => {
            var html = '';
            for (var i = 0; i < x.length; i++) {
                html += '<tr><td>' + x[i] + '</td></tr>';
            }
            $('#lstAssets').html(html);
        });
    }
}, 2000);
