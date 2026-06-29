let $island = null;
let islandTimer = null;
let $measureEl = null;

// 拖拽变量
let isDrag = false;
let dragStartX = 0;
let dragStartY = 0;
let offsetX = 0;
let offsetY = 0;

// 初始化：页面加载立刻执行，一启动就生成灵动岛，默认文字 TexNEL
function initDynamicIsland() {
  if ($island) return;

  // 创建灵动岛DOM，初始文字直接赋值 TexNEL
  $island = document.createElement('div');
  $island.className = 'dynamic-island-wrap';
  $island.textContent = "TexNEL";
  document.body.appendChild($island);

  // 创建文字测量元素
  $measureEl = document.createElement('span');
  $measureEl.id = 'text-measure-ghost';
  document.body.appendChild($measureEl);

  // 拖拽监听绑定
  $island.addEventListener('mousedown', startDrag);
  document.addEventListener('mousemove', moveDrag);
  document.addEventListener('mouseup', stopDrag);
}

// 测量文字宽度
function getTextRealWidth(text) {
  $measureEl.textContent = text;
  return $measureEl.offsetWidth;
}

// 拖拽按下
function startDrag(e) {
  isDrag = true;
  $island.style.transition = 'none';
  const rect = $island.getBoundingClientRect();
  dragStartX = e.clientX;
  dragStartY = e.clientY;
  offsetX = dragStartX - rect.left;
  offsetY = dragStartY - rect.top;
}

// 拖拽移动
function moveDrag(e) {
  if (!isDrag) return;
  let x = e.clientX - offsetX;
  let y = e.clientY - offsetY;
  x = Math.max(0, Math.min(x, window.innerWidth - $island.offsetWidth));
  y = Math.max(0, Math.min(y, window.innerHeight - $island.offsetHeight));

  $island.style.left = `${x}px`;
  $island.style.top = `${y}px`;
  $island.style.transform = 'none';
}

// 结束拖拽
function stopDrag() {
  if (!isDrag) return;
  isDrag = false;
  $island.style.transition = '';
}

// 对外调用方法，完全兼容旧代码
function showToast(message, level = 'info', duration = 3000) {
  initDynamicIsland();
  clearTimeout(islandTimer);

  $island.className = 'dynamic-island-wrap';
  $island.classList.add(`island-${level}`);
  $island.textContent = message;

  const textRealW = getTextRealWidth(message);
  const paddingTotal = 36;
  let expandWidth = textRealW + paddingTotal;
  const minExpandW = 190;
  const maxExpandW = window.innerWidth * 0.88;
  expandWidth = Math.max(minExpandW, Math.min(expandWidth, maxExpandW));

  requestAnimationFrame(() => {
    $island.style.width = `${expandWidth}px`;
    $island.classList.add('island-expand');
  });

  // 倒计时收缩，恢复默认绿底 + TexNEL
  islandTimer = setTimeout(() => {
    $island.classList.remove('island-expand');
    setTimeout(() => {
      $island.textContent = 'TexNEL';
      $island.style.width = '';
      // 恢复常态绿底基础类
      $island.className = 'dynamic-island-wrap';
    }, 400);
  }, duration);
}

// 页面加载立即初始化，打开页面直接显示绿底TexNEL灵动岛
document.addEventListener('DOMContentLoaded', initDynamicIsland);