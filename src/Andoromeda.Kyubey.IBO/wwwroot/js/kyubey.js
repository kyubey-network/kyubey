var chain_id = 'aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906';
var host = 'nodes.get-scatter.com';
var account, eos, requiredFields;

function showModal(title, content) {
    $('#modalTitle').text(title);
    $('#modalContent').text(content);
    $('#modal').modal('show');
}

function login() {
    if (!('scatter' in window)) {
        alert('Scatter插件没有找到', 'Scatter是一款EOS钱包，运行在Chrome浏览器中，请您确定已经安装Scatter插件. 参考：https://www.jianshu.com/p/a2e1e6204023');
        return Promise.reject();
    } else {
        var network = {
            blockchain: 'eos',
            host: host,
            port: 443,
            protocol: 'https',
            chainId: chain_id
        };

        return scatter.getIdentity({ accounts: [network] }).then(identity => {
            account = identity.accounts.find(acc => acc.blockchain === 'eos');
            eos = scatter.eos(network, Eos, {});
            requiredFields = { accounts: [network] };
        });
    }
}

function ensureLogin() {
    if (account)
        return Promise.resolve();

    return login();
}

if ($('#form').length > 0) {
    setTimeout(login, 2000);
}

$('#btnBuy').click(function () {
    ensureLogin()
        .then(() => {
            eos.contract('eosio.token', { requiredFields })
                .then(contract => {
                    return contract.transfer(
                        account.name,
                        'myeosgroupon',
                        parseFloat($('#amount').val()).toFixed(4) + ' EOS',
                        '',
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                });
        });
});

$('#btnSell').click(function () {
    ensureLogin()
        .then(() => {
            eos.contract('dacincubator', { requiredFields })
                .then(contract => {
                    return contract.transfer(
                        account.name,
                        'dacincubator',
                        parseFloat($('#amount').val()).toFixed(4) + ' KBY',
                        'sell',
                        {
                            authorization: [`${account.name}@${account.authority}`]
                        });
                });
        });
});