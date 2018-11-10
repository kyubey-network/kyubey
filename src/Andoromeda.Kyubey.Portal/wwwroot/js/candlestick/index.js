

// 单纯的写一个添加class的函数，这个不用看 没用
function addClass (fatherDom, dom) {
  [...fatherDom.getElementsByTagName('span')].forEach(function(item){
    item.className = ''
  })
  dom.className = 'active'
}