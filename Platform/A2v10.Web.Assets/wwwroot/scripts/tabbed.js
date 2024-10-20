// Copyright © 2023 Oleksandr Kukhtin. All rights reserved.

/*20230605-8109*/
app.modules['std:signalR'] = function () {

	const eventBus = require('std:eventBus')

	const connection = new signalR.HubConnectionBuilder()
		.withUrl("/_userhub")
		.withAutomaticReconnect()
		.configureLogging(signalR.LogLevel.Information)
		.build();

	connection.on('signal', (event, data) => {
		eventBus.$emit('signalEvent', { event, data })
	});

	return {
		startService,
	};

	async function startService() {
		try {
			await connection.start();
			console.log("SignalR Connected.");
		} catch (err) {
			console.log(err);
			setTimeout(start, 5000);
		}
	}
};

// Copyright © 2023 Oleksandr Kukhtin. All rights reserved.

/*20230525-8100*/
/* tabbed:appheader.js */
(function () {

	const locale = window.$$locale;
	const http = require("std:http");
	const eventBus = require("std:eventBus");

	Vue.component("a2-mdi-header", {
		template: `
	<div class="mdi-header">
		<div class="app-logo" v-if="hasLogo">
			<img :src="logo"></img>
		</div>
		<div v-else class="app-title" v-text=title></div>
		<div class="aligner"></div>
		<slot></slot>
		<div class="dropdown dir-down separate" v-dropdown>
			<button class="user-name" toggle :title="personName"><i class="ico ico-user"></i> 
				<span id="layout-person-name" class="person-name" v-text="personName"></span>
				<span class="caret"></span>
			</button>
			<div class="dropdown-menu menu down-left">
				<button v-if=hasProfile @click=profile tabindex="-1" class="dropdown-item">
					<i class="ico ico-user"></i> 
					<span v-text="profileText"></span>
				</button>
				<button @click=logout tabindex="-1" class="dropdown-item">
					<i class="ico ico-logout"></i> 
					<span v-text="locale.$Quit"></span>
				</button>
			</div>
		</div>
	</div>
		`,
		props: {
			title: String,
			subTitle: String,
			personName: String,
			hasProfile: Boolean,
			profileText: String,
			logo: String
		},
		computed: {
			locale() { return locale; },
			hasLogo() { return !!this.logo; }
		},
		methods: {
			async logout() {
				let res = await http.post('/account/logout2');
				window.location.assign(`/account/${res.showLogOut ? 'loggedout' : 'login'}`);
			},
			profile() {
				eventBus.$emit('navigateto', { url: '/_profile/index/0'});
			}
		},
		mounted() {
		}
	});
})();
// Copyright © 2023-2024 Oleksandr Kukhtin. All rights reserved.

/*20240807-8333*/
/* tabbed:navbar.js */
(function () {

	const popup = require('std:popup');
	const eventBus = require('std:eventBus');

	Vue.component("a2-mdi-navbar", {
		template: `
<div class="mdi-navbar">
	<ul class="bar">
		<li v-for="m in menu" @click.stop.prevent=clickMenu(m) :title=m.Name :class="menuClass(m)">
			<i class="ico" :class="menuIcon(m)"></i>
			<span v-text="m.Name" class="menu-text"></span>
		</li>
	</ul>
	<div class="mdi-menu" v-if="isMenuVisible">
		<div class="menu-title" v-text=activeMenu.Name></div>
		<ul>
			<li v-for="m in activeMenu.Menu" class="level-0">
				<span class="folder" v-text="m.Name"></span>
				<ul v-if="!!m.Menu">
					<li v-for="im in m.Menu" class="level-1" @click.stop.prevent="clickSubMenu(im.Url, im.Name)">
						<span v-text="im.Name"></span>
						<button v-if="im.CreateUrl" class="btn-plus ico ico-plus-circle" 
							:title="im.CreateName" @click.stop.prevent="clickSubMenu(im.CreateUrl, im.CreateName)"></button>
					</li>
				</ul>
			</li>
		</ul>
	</div>
</div>
	`,
		props: {
			menu: Array,
		},
		data() {
			return {
				activeMenu: null
			};
		},
		computed: {
			isMenuVisible() {
				return !!this.activeMenu;
			}
		},
		methods: {
			clickMenu(m) {
				let am = this.activeMenu;
				eventBus.$emit('closeAllPopups');
				const shell = this.$parent;
				if (m.ClassName === 'grow')
					return;
				if (!m.Menu) {
					this.activeMenu = null;
					shell.$emit('navigate', { title: m.Name, url: m.Url });
				} else if (am === m) {
					this.activeMenu = null;
				} else {
					this.activeMenu = m;
				}
			},
			clickSubMenu(url, title) {
				eventBus.$emit('closeAllPopups');
				const shell = this.$parent;
				this.activeMenu = null;
				if (url.startsWith("page:"))
					shell.$emit('navigate', { title: title, url: url.substring(5) });
				else if (url.startsWith("dialog:")) {
					const dlgData = { promise: null, rd: true, direct: true };
					eventBus.$emit('modal', url.substring(7), dlgData);
				} else
					alert('invalid menu url: ' + url);
			},
			menuClass(m) {
				return (m.ClassName ? m.ClassName : '') + ((m === this.activeMenu) ? ' active' : '');
			},
			menuIcon(m) {
				return 'ico-' + m.Icon;
			},
			__clickOutside() {
				this.activeMenu = null;
			}
		},
		mounted() {
			popup.registerPopup(this.$el);
			this.$el._close = this.__clickOutside;
		}
	});
})();

// Copyright © 2023-2024 Oleksandr Kukhtin. All rights reserved.

/*20240820-8337*/

/* tabbed:shell.js */
(function () {
	const eventBus = require('std:eventBus');
	const popup = require('std:popup');
	const utils = require('std:utils');
	const urlTools = require('std:url');
	const log = require('std:log');
	const signalR = require('std:signalR');

	const modalComponent = component('std:modal');
	const toastrComponent = component('std:toastr');
	const store = component('std:store');

	let tabKey = 77;

	const tabUrlKey = tab => `${tab.url}:${tab.key}`;

	app.components["std:shellPlain"] = Vue.extend({
		store,
		data() {
			return {
				tabs: [],
				closedTabs: [],
				activeTab: null,
				modals: [],
				modalRequeryUrl: '',
				traceEnabled: log.traceEnabled(),
				debugShowTrace: false,
				debugShowModel: false,
				dataCounter: 0,
				sidePaneUrl: '',
				tabPopupOpen: false,
				navigatingUrl: '',
				homePageTitle: '',
				homeLoaded: false,
				lockRoute: false,
				requestsCount: 0,
				contextTabKey: 0,
				newVersionAvailable: false
			};
		},
		components: {
			'a2-modal': modalComponent,
			'a2-toastr': toastrComponent
		},
		computed: {
			modelStack() { return this.__dataStack__; },
			hasModals() { return this.modals.length > 0; },
			sidePaneVisible() { return !!this.sidePaneUrl; },
			storageKey() { return `${this.appData.appId}_${this.appData.userId}_tabs`; },
			processing() { return !this.hasModals && this.requestsCount > 0; },
			canPopupClose() { return this.contextTabKey > 10; /* 10 - home */ },
			canPopupCloseRight() { return this.contextTabKey && this.tabs.some(v => v.key > this.contextTabKey); },
			canReopenClosed() { return this.closedTabs.length > 0 },
			hasModified() { return this.tabs.some(t => t.root && t.root.$isDirty); },
			maxTabWidth() { return `calc((100% - 50px) / ${this.tabs.length})`; }
		},
		methods: {
			replaceState(tab) {
				//??window.history.replaceState(null, null, tab ? tab.url : '/');
			},
			navigate(u1) {
				if (u1.url.indexOf('{genrandom}') >= 0) {
					let randomString = Math.random().toString(36).substring(2);
					u1.url = u1.url.replace('{genrandom}', randomString);
				}
				let tab = this.tabs.find(tab => tab.url == u1.url);
				if (!tab) {
					let parentUrl = '';
					if (this.activeTab)
						parentUrl = this.activeTab.url || '';
					tab = { title: u1.title, url: u1.url, query: u1.query || '', loaded: true, key: tabKey++, root: null, parentUrl: parentUrl, reload: 0, debug: false };
					this.tabs.push(tab);
					var cti = this.closedTabs.findIndex(t => t.url === u1.url);
					if (cti >= 0)
						this.closedTabs.splice(cti, 1);
				}
				tab.loaded = true;
				this.activeTab = tab;
				if (this.tabs.length > 10)
					this.tabs.splice(0, 1);
				this.storeTabs();
			},
			navigateUrl(url) {
				this.navigatingUrl = url;
				this.navigate({ url: url, title: '' });
			},
			navigateTo(to) {
				this.navigatingUrl = to.url;
				this.navigate({ url: to.url, title: '' });
			},
			dummy() { },
			reloadApplication() {
				window.location.reload();
			},
			setDocTitle(title) {
				let tab = this.activeTab;
				if (!tab && this.navigatingUrl)
					tab = this.tabs.find(tab => tab.url === this.navigatingUrl);
				if (tab) {
					tab.title = title;
					document.title = title;
				} else {
					this.homePageTitle = title;
					document.title = title;
				}
			},
			setNewId(route) {
				let tab = this.tabs.find(tab => tab.url === route.from);
				if (!tab) return;
				this.lockRoute = true;
				tab.url = route.to;
				if (tab.root)
					tab.root.__tabUrl__ = tabUrlKey(tab);
				Vue.nextTick(() => {
					this.lockRoute = false;
				});
				this.storeTabs();
			},
			tabLoadComplete(page) {
				if (page) {
					let tab = this.activeTab || this.tabs.find(t => t.url == page.src);
					if (tab) {
						this.replaceState(tab);
						tab.root = page.root;
						document.title = tab.title;
						if (page.root) {
							page.root.__tabUrl__ = tabUrlKey(tab);
							page.root.$store.commit('setroute', tab.url);
						}
					}
				}
				this.navigatingUrl = '';
			},
			isTabActive(tab) {
				return tab === this.activeTab;
			},
			isHomeActive() {
				return !this.activeTab;
			},
			fitText(t) {
				if (!t) return 'untitled';
				return t.length > 30 ? t.substring(0, 30) + '…' : t;
			},
			tabTitle(tab) {
				let star = '';
				if (tab.root && tab.root.$isDirty)
					star = '* ';
				return star + tab.title;

			},
			tabSource(tab) {
				return tab.loaded ? (tab.url + (tab.query || '')) : null;
			},
			homeSource() {
				return this.homeLoaded ? '/_home/index/0' : null;
			},
			selectHome(noStore) {
				this.homeLoaded = true;
				this.activeTab = null;
				document.title = this.homePageTitle;
				this.replaceState(null);
				if (noStore)
					return;
				this.storeTabs();
			},
			selectTab(tab, noStore, ev) {
				eventBus.$emit('closeAllPopups');
				if (ev && ev.ctrlKey) {
					tab.debug = true;
					return;
				}
				tab.loaded = true;
				this.activeTab = tab;
				document.title = tab.title;
				this.replaceState(tab);
				if (noStore)
					return;
				this.storeTabs();
			},
			reopenTab(tab) {
				eventBus.$emit('closeAllPopups');
				this.navigate(tab);
			},
			closeTabFromStore(state) {
				if (!state || !state.root) return;
				let route = state.root.__tabUrl__;
				let tabIndex = this.tabs.findIndex(t => tabUrlKey(t) === route);
				if (tabIndex >= 0)
					this.removeTab(tabIndex);
			},
			closeTab(tab) {
				eventBus.$emit('closeAllPopups');
				let tabIndex = this.tabs.indexOf(tab);
				if (tabIndex == -1)
					return;
				if (tab !== this.activeTab)
					; // do nothing
				if (tab.root && tab.root.$close)
					tab.root.$close();
				else
					this.removeTab(tabIndex);
			},
			removeTabs(ta) {
				if (!ta || !ta.length) return;
				ta.forEach(tab => {
					let ix = this.tabs.indexOf(tab);
					if (ix == -1) return;
					let rt = this.tabs.splice(ix, 1);
					if (rt.length) {
						this.closedTabs.unshift(rt[0]);
						if (this.closedTabs.length > 10)
							this.closedTabs.pop();
					}
				});
				if (!this.tabs.length)
					this.selectHome(true);
				else {
					let currentTab = this.tabs.find(t => t.key === this.contextTabKey);
					if (this.activeTab !== currentTab)
						this.selectTab(currentTab, true);
				}
				this.storeTabs();
			},
			removeTab(tabIndex) {
				let currentTab = this.tabs[tabIndex];
				let parent = this.tabs.find(t => t.url === currentTab.parentUrl);
				if (parent)
					this.selectTab(parent, true);
				else if (tabIndex > 0)
					this.selectTab(this.tabs[tabIndex - 1], true);
				else if (this.tabs.length > 1)
					this.selectTab(this.tabs[tabIndex + 1], true);
				else
					this.selectHome(true);
				let rt = this.tabs.splice(tabIndex, 1);
				if (rt.length) {
					this.closedTabs.unshift(rt[0]);
					if (this.closedTabs.length > 10)
						this.closedTabs.pop();
				}
				this.storeTabs();
			},
			clearLocalStorage() {
				let keys = [];
				for (let f in window.localStorage) {
					if (window.localStorage.hasOwnProperty(f) && f.endsWith('_tabs')) {
						if (f != this.storageKey)
							keys.push(f);
					}
				}
				if (keys.length > 10)
					keys.forEach(f => window.localStorage.removeItem(f));
			},
			storeTabs() {
				var mapTab = (t) => { return { title: t.title, url: t.url, query: t.query || '', parentUrl: t.parentUrl }; };
				let ix = this.tabs.indexOf(this.activeTab);
				let tabs = JSON.stringify({
					index: ix,
					tabs: this.tabs.map(mapTab),
					closedTabs: this.closedTabs.map(mapTab),
				});
				window.localStorage.setItem(this.storageKey, tabs);
			},
			restoreTabs(path) {
				let newurl = path !== '/' ? path : '';
				let tabs = window.localStorage.getItem(this.storageKey);
				if (!tabs) {
					this.selectHome(true);
					return;
				}
				try {
					let elems = JSON.parse(tabs);
					let ix = elems.index;
					for (let i = 0; i < elems.tabs.length; i++) {
						let t = elems.tabs[i];
						let loaded = ix === i;
						if (loaded)
							this.navigatingUrl = t.url;
						this.tabs.push({ title: t.title, url: t.url, query: t.query, loaded, key: tabKey++, root: null, parentUrl: t.parentUrl, reload: 0, debug: false });
					}
					for (let i = 0; i < elems.closedTabs.length; i++) {
						let t = elems.closedTabs[i];
						this.closedTabs.push({ title: t.title, url: t.url, query: t.query, loaded: true, key: tabKey++ });
					}
					if (ix >= 0 && ix < this.tabs.length)
						this.activeTab = this.tabs[ix];
					else
						this.selectHome(true);
				} catch (err) {
				}
			},
			toggleTabPopup() {
				eventBus.$emit('closeAllPopups');
				this.tabPopupOpen = !this.tabPopupOpen;
			},
			// context menu
			tabsContextMenu(ev) {
				eventBus.$emit('closeAllPopups');
				this.contextTabKey = 0;
				let li = ev.target.closest('li');
				if (li)
					this.contextTabKey = +(li.getAttribute('tab-key') || 0);
				let menu = document.querySelector('#ctx-tabs-popup');
				let br = menu.parentNode.getBoundingClientRect();
				let mw = 242; // menu width
				let lp = ev.clientX - br.left;
				if (lp + mw > br.right)
					lp -= mw;
				let style = menu.style;
				style.top = (ev.clientY - br.top) + 'px';
				style.left = lp + 'px';
				menu.classList.add('show');
				//console.dir(this.contextTabKey);
			},
			popupClose() {
				let t = this.tabs.find(t => t.key === this.contextTabKey);
				if (t) this.closeTab(t);
			},
			reopenClosedTab() {
				if (this.closedTabs.length <= 0) return;
				let t = this.closedTabs[0];
				this.reopenTab(t);
			},
			popupCloseOther() {
				if (!this.contextTabKey) return;
				let tabs = this.tabs.filter(t => t.key !== this.contextTabKey);
				this.removeTabs(tabs);
			},
			popupCloseRight() {
				if (!this.contextTabKey) return;
				let tabs = this.tabs.filter(t => t.key > this.contextTabKey);
				this.removeTabs(tabs);
			},
			popupCloseAll() {
				this.selectHome(true);
				let tabs = this.tabs.filter(t => t.key > 10);
				this.removeTabs(tabs);
			},
			showModal(modal, prms) {
				let id = utils.getStringId(prms ? prms.data : null);
				let raw = prms && prms.raw;
				let direct = prms && prms.direct;
				let root = window.$$rootUrl;
				let url = urlTools.combine(root, '/_dialog', modal, id);
				if (raw)
					url = urlTools.combine(root, modal, id);
				else if (direct)
					url = urlTools.combine(root, '/_dialog', modal);
				url = store.replaceUrlQuery(url, prms.query);
				let dlg = { title: "dialog", url: url, prms: prms.data, wrap: false, rd: prms.rd };
				dlg.promise = new Promise(function (resolve, reject) {
					dlg.resolve = resolve;
				});
				prms.promise = dlg.promise;
				this.modals.push(dlg);
				this.setupWrapper(dlg);
			},
			setupWrapper(dlg) {
				this.modalRequeryUrl = '';
				setTimeout(() => {
					dlg.wrap = true;
					//console.dir("wrap:" + dlg.wrap);
				}, 50); // same as modal
			},
			_requery(vm, data) {
				if (!vm) return;
				if (vm.__requery) {
					vm.__requery();
				}
				if (!vm.__tabUrl__) return;
				let t = this.tabs.find(t => tabUrlKey(t) === vm.__tabUrl__);
				if (!t) return;
				let run = data ? data.Run : false;
				if (data)
					data.Run = undefined;
				if (!t.loaded)
					return;
				t.url = urlTools.replaceUrlQuery(t.url, data)
				if (t.root)
					t.root.__tabUrl__ = tabUrlKey(t);
				t.reload += run ? 7 : 1;
				this.storeTabs();
			},
			_isModalRequery(arg) {
				if (arg.url && this.modalRequeryUrl && this.modalRequeryUrl === arg.url)
					arg.result = true;
			},
			_modalRequery(baseUrl) {
				let dlg = this._findRealDialog();
				if (!dlg) return;
				let inst = dlg.instance; // include instance
				if (inst && inst.modalRequery) {
					if (baseUrl)
						dlg.url = baseUrl;
					this.modalRequeryUrl = dlg.url;
					inst.modalRequery();
				}
			},
			_modalCreated(instance) {
				// include instance!
				let dlg = this._findRealDialog();
				if (!dlg) return;
				dlg.instance = instance;
			},
			_eventModalClose(result) {
				if (!this.modals.length) return;

				const dlg = this.modals[this.modals.length - 1];

				const closeImpl = (closeResult) => {
					let dlg = this.modals.pop();
					if (closeResult)
						dlg.resolve(closeResult);
				}

				if (!dlg.attrs) {
					closeImpl(result);
					return;
				}

				if (dlg.attrs.alwaysOk)
					result = true;

				if (dlg.attrs.canClose) {
					let canResult = dlg.attrs.canClose();
					if (canResult === true)
						closeImpl(result);
					else if (canResult.then) {
						result.then(function (innerResult) {
							if (innerResult === true)
								closeImpl(result);
							else if (innerResult) {
								closeImpl(innerResult);
							}
						});
					}
					else if (canResult)
						closeImpl(canResult);
				} else {
					closeImpl(result);
				}
			},
			_eventModalCloseAll() {
				while (this.modals.length) {
					let dlg = this.modals.pop();
					dlg.resolve(false);
				}
			},
			_eventConfirm(prms) {
				let dlg = prms.data;
				dlg.wrap = false;
				dlg.promise = new Promise(function (resolve) {
					dlg.resolve = resolve;
				});
				prms.promise = dlg.promise;
				this.modals.push(dlg);
				this.setupWrapper(dlg);
			},
			_pageReloaded(path) {
				if (!path) return;
				let seg = path.split('?');
				if (seg.length < 2) return;
				let tab = this.tabs.find(t => '/_page' + t.url === seg[0]);
				if (!tab) return;
				let q = '';
				if (seg[1])
					q = '?' + seg[1];
				if (tab.query === q) return;
				this.lockRoute = true;
				tab.query = q;
				Vue.nextTick(() => {
					this.lockRoute = false;
				});
				this.storeTabs();
			},
			_eventToParentTab(ev) {
				let tab = this.tabs.find(t => t.root === ev.source);
				if (!tab || !tab.root || !tab.parentUrl) return;
				let ptab = this.tabs.find(t => t.url === tab.parentUrl);
				if (ptab && ptab.root)
					ptab.root.$data.$emit(ev.event, ev.data);
			},
			_findRealDialog() {
				// skip alerts, confirm, etc
				for (let i = this.modals.length - 1; i >= 0; --i) {
					let md = this.modals[i];
					if (md.rd) {
						return md;
					}
				}
				return null;
			},
			debugTrace() {
				if (!window.$$debug) return;
				this.debugShowModel = false;
				this.debugShowTrace = !this.debugShowTrace;
			},
			debugModel() {
				if (!window.$$debug) return;
				this.debugShowTrace = false;
				this.debugShowModel = !this.debugShowModel;
			},
			debugClose() {
				this.debugShowModel = false;
				this.debugShowTrace = false;
			},
			registerData(component, out) {
				this.dataCounter += 1;
				if (component) {
					if (this.__dataStack__.length > 0)
						out.caller = this.__dataStack__[0];
					this.__dataStack__.unshift(component);
				}
				else if (this.__dataStack__.length > 1)
					this.__dataStack__.shift();
			},
			updateModelStack(root) {
				if (this.__dataStack__.length > 0 && this.__dataStack__[0] === root)
					return;
				this.__dataStack__.splice(0);
				this.__dataStack__.unshift(root);
				this.dataCounter += 1; // refresh
			},
			showSidePane(url) {
				if (!url) {
					this.sidePaneUrl = '';
				} else {
					let newurl = '/_page' + url;
					if (this.sidePaneUrl === newurl)
						this.sidePaneUrl = '';
					else
						this.sidePaneUrl = newurl;
				}
			},
			__clickOutside() {
				this.tabPopupOpen = false;
			}
		},
		watch: {
			traceEnabled(val) {
				log.enableTrace(val);
			},
			activeTab(newtab) {
				if (newtab && newtab.root)
					this.updateModelStack(newtab.root)
			}
		},
		mounted() {
			popup.registerPopup(this.$el);
			// home page here this.tabs.push({})
			this.$el._close = this.__clickOutside;
			this.clearLocalStorage();
			if (!this.menu || !this.menu.length)
				this.selectHome(false); // store empty tabs
			else
				this.restoreTabs(window.location.pathname + window.location.search);
		},
		created() {
			const me = this;
			me.__dataStack__ = [];
			popup.startService();
			this.$on('navigate', this.navigate);
			eventBus.$on('navigateto', this.navigateTo);
			eventBus.$on('setdoctitle', this.setDocTitle);
			eventBus.$on('setnewid', this.setNewId);
			eventBus.$on('closeAllPopups', popup.closeAll);
			eventBus.$on('modal', this.showModal);
			eventBus.$on('modalCreated', this._modalCreated);
			eventBus.$on('requery', this._requery);
			eventBus.$on('isModalRequery', this._isModalRequery);
			eventBus.$on('modalRequery', this._modalRequery);
			eventBus.$on('modalClose', this._eventModalClose);
			eventBus.$on('modalCloseAll', this._eventModalCloseAll);
			eventBus.$on('registerData', this.registerData);
			eventBus.$on('showSidePane', this.showSidePane);
			eventBus.$on('confirm', this._eventConfirm);
			eventBus.$on('closePlain', this.closeTabFromStore);
			eventBus.$on('pageReloaded', this._pageReloaded);
			eventBus.$on('toParentTab', this._eventToParentTab);
			eventBus.$on('closeAllTabs', this.popupCloseAll);
			eventBus.$on('beginRequest', () => {
				me.requestsCount += 1;
				window.__requestsCount__ = me.requestsCount;
			});
			eventBus.$on('endRequest', () => {
				me.requestsCount -= 1;
				window.__requestsCount__ = me.requestsCount;
			});
			eventBus.$on('checkVersion', (ver) => {
				if (ver && this.appData && (ver.app !== this.appData.version || ver.module !== this.appData.moduleVersion))
					this.newVersionAvailable = true;
			});

			signalR.startService();

			window.addEventListener("beforeunload", (ev) => {
				if (!this.hasModified)
					return;
				ev.preventDefault();
				if (ev) ev.returnValue = true;
				return true;
			});
		}
	});
})();

// Copyright © 2024 Oleksandr Kukhtin. All rights reserved.
(function () {

	const TODAY_COLOR = "#fffff080"; // sync with LESS!
	const locale = window.$$locale;
	const dateLocale = locale.$DateLocale || locale.$Locale;
	const monthLocale = locale.$Locale; // for text

	let utils = require('std:utils');
	let tu = utils.text;
	let du = utils.date;

	Vue.component('a2-big-calendar', {
		template: `
<div class="a2-big-calendar">
<div class="top-bar">
	<div class="toolbar">
		<button class="btn btn-tb btn-icon" @click="nextPart(-1)"><i class="ico ico-arrow-left"></i></button>
		<button class="btn btn-tb btn-icon" @click="nextPart(1)"><i class="ico ico-arrow-right"></i></button>
		<button class="btn btn-tb btn-icon" @click="todayPart"><i class="ico ico-calendar-today"></i> <span v-text="locale.$Today"></span></button>
	</div>
	<div class="title">
		<span v-text=topTitle></span>
	</div>
	<div class="toolbar">
		<button class="btn btn-tb btn-icon" @click="setView('week')"><i class="ico ico-calendar-week"></i> <span v-text="locale.$Week"></span></button>
		<button class="btn btn-tb btn-icon" @click="setView('month')"><i class="ico ico-calendar"></i> <span v-text="locale.$Month"></span></button>
		<slot name="topbar"></slot>
	</div>
</div>
<div v-if="isView('month')" class="bc-month-conainer bc-container" ref="mc">
<table class="bc-month bc-table">
	<thead>
		<tr class="weekdays"><th v-for="d in 7" v-text="wdTitle(d)"></th></tr>
	</thead>
	<tbody>
		<tr v-for="row in days">
			<td v-for="day in row" class=bc-day :class="dayClass(day)" @click.stop.prevent="clickDay(day)">
				<div v-text="day.getDate()" class="day-date"></div>
				<div class="bc-day-body">
					<div v-for="(ev, ix) in dayEvents(day)" :key=ix class="day-event"
							:style="{backgroundColor: ev.Color || ''}" @click.stop.prevent="clickEvent(ev)">
						<slot name="monthev" v-bind:item="ev" class="me-body">
							<span class="me-body" v-text="eventTitle(ev)" :title="eventTitle(ev)"/>
						</slot>
					</div>
				</div>
			</td>
		</tr>
	</tbody>
</table>
</div>
<div v-if="isView('week')" class="bc-week-container bc-container" ref=wc>
	<table class="bc-week bc-table">
		<colgroup>
			<col style="width:2%"/>
			<col v-for="d in 7" style="width:14%" :style="weekColumnStyle(d)">
		</colgroup>
		<thead>
			<tr class="weekdays">
				<th></th>
				<th v-for="d in 7" v-text="wdWeekTitle(d)" :class="{today: isWeekDayToday(d)}"></th>
			</tr>
		</thead>
		<tbody>
			<template v-for="h in 24">
				<tr>
					<th rowspan=2>
						<span v-text="hoursText(h - 1)" class="h-title"></span>
					</th>
					<td v-for="d in 7" class="bc-h-top" @click.stop.prevent="clickHours(d, h, false)">
						<div class="h-event" v-for="(ev, ix) in hourEvents(d, h-1, false)" :key=ix
								:style="hourStyle(ev, false)" @click.stop.prevent="clickEvent(ev)">
							<slot name="weekev" v-bind:item="ev">
								<span class="h-ev-body" v-text="eventTitle(ev)" :title="eventTitle(ev)"></span>
							</slot>
						</div>
					</td>
				</tr>
				<tr>
					<td v-for="d in 7" class="bc-h-bottom" @click.stop.prevent="clickHours(d, h, true)">
						<div class="h-event" v-for="(ev, ix) in hourEvents(d, h-1, true)" :key=ix
							:style="hourStyle(ev, true)" @click.stop.prevent="clickEvent(ev)">
							<slot name="weekev" v-bind:item="ev">
								<span class="h-ev-body" v-text="eventTitle(ev)" :title="eventTitle(ev)"></span>
							</slot>
						</div>
					</td>
				</tr>
			</template>
		</tbody>
	</table>
	<div class="current-time" :style="currentTimeStyle()" :key=updateKey></div>
</div>
</div>
`,
		props: {
			item: Object,
			prop: String,
			viewItem: Object,
			viewProp: String,
			events: Array,
			delegates: Object
		},
		data() {
			return {
				updateKey: 1,
				timerId: 0
			}
		},
		computed: {
			locale() {
				return locale;
			},
			modelDate() {
				return this.item[this.prop];
			},
			modelView() {
				return this.viewItem[this.viewProp];
			},
			days() {
				let dt = new Date(this.modelDate);
				let d = dt.getDate();
				dt.setDate(1); // 1-st day of month
				let w = dt.getDay() - 1; // weekday
				if (w === -1) w = 6;
				//else if (w === 0) w = 7;
				dt.setDate(-w + 1);
				let arr = [];
				for (let r = 0; r < 6; r++) {
					let row = [];
					for (let c = 0; c < 7; c++) {
						row.push(new Date(dt));
						dt.setDate(dt.getDate() + 1);
					}
					arr.push(row);
				}
				return arr;
			},
			topTitle() {
				let m = this.modelDate.toLocaleDateString(monthLocale, { month: "long" });
				return `${tu.capitalize(m)} ${this.modelDate.getFullYear()}`;
			},
			firstMonday() {
				let wd = this.modelDate.getDay();
				if (wd == 1) return this.modelDate;
				if (!wd) wd = 7;
				return du.add(this.modelDate, -(wd - 1), 'day');
			}
		},
		methods: {
			isView(view) {
				return this.viewItem[this.viewProp] === view;
			},
			wdTitle(d) {
				let dt = this.days[0][d - 1];
				return dt.toLocaleString(monthLocale, { weekday: "long" });
			},
			wdWeekTitle(d) {
				let fd = du.add(this.firstMonday, d - 1, 'day');
				let wd = fd.toLocaleString(monthLocale, { weekday: 'long' });
				let day = fd.toLocaleString(monthLocale, { month: "long", day: 'numeric' });
				return `${wd}, ${day}`;
			},
			dayClass(day) {
				let cls = '';
				if (du.equal(day, du.today()))
					cls += ' today';
				if (day.getMonth() !== this.modelDate.getMonth())
					cls += " other";
				return cls;
			},
			dayEvents(day) {
				return this.events.filter(e => du.equal(e.Date, day));
			},
			hoursText(h) {
				return `${h}:00`;
			},
			hourStyle(ev, h2) {
				let min = ev.Date.getMinutes();
				let s = {
					backgroundColor: ev.Color,
					height: `${ev.Duration}px`,
					top: `${h2 ? min - 30 : min}px`
				};
				return s;
			},
			hourEvents(dno, hour, h2) {
				let day = du.add(this.firstMonday, dno - 1, 'day');
				let inside = (ev) => {
					if (!du.equal(ev.Date, day)) return false;
					let h = ev.Date.getHours();
					let m = ev.Date.getMinutes();
					if (h2)
						return h >= hour && h < hour + 1 && m >= 30;
					else
						return h >= hour && h < hour + 1 && m < 30;
				}
				return this.events.filter(inside);
			},
			nextPart(d) {
				let dt = new Date(this.modelDate);
				if (this.isView('week'))
					dt = du.add(dt, 7 * d, 'day');
				else
					dt.setMonth(dt.getMonth() + d);
				this.item[this.prop] = dt;
			},
			todayPart() {
				let dt = new Date();
				this.item[this.prop] = dt;
			},
			setView(view) {
				this.viewItem[this.viewProp] = view;
			},
			clickEvent(ev) {
				if (this.delegates && this.delegates.ClickEvent)
					this.delegates.ClickEvent(ev);
			},
			clickDay(day) {
				if (!this.delegates || !this.delegates.ClickDay) return;
				let dt = new Date(day);
				dt.setHours(0);
				dt.setMinutes(0);
				this.delegates.ClickDay(day);
			},
			clickHours(d, h, h2) {
				if (!this.delegates || !this.delegates.ClickDay) return;
				let fmd = this.firstMonday;
				let dt = du.add(fmd, d - 1, 'day');
				dt.setMinutes(h2 ? 30 : 0);
				dt.setHours(h - 1);
				this.delegates.ClickDay(dt);
			},
			currentTimeStyle() {
				let dt = new Date();
				let x = dt.getHours() * 60 + dt.getMinutes() + 35 - 1 /*thead*/;
				return {
					top: `${x}px`,
					'--time': `"${dt.toLocaleTimeString(dateLocale, { hour: '2-digit', minute: '2-digit' })}"`
				};
			},
			isWeekDayToday(d) {
				let day = du.add(this.firstMonday, d - 1, 'day');
				return du.equal(day, du.today());
			},
			weekColumnStyle(d) {
				if (this.isWeekDayToday(d))
					return { backgroundColor: TODAY_COLOR };
				return undefined;
			},
			eventTitle(ev) {
				let time = ev.Date.toLocaleTimeString(dateLocale, { hour: '2-digit', minute: '2-digit' });
				return `${time} ${ev.Name}`;
			},
			__fitScroll() {
				if (this.isView('month')) {
					setTimeout(() => {
						this.$refs.mc.scrollTop = 0;
					}, 0);
				} else
					setTimeout(() => {
						let rows = this.$refs.wc.getElementsByTagName('tr');
						rows[8 * 2 + 1].scrollIntoView(true);
					}, 0);
			}
		},
		watch: {
			modelView() {
				this.__fitScroll();
			}
		},
		mounted() {
			this.__fitScroll();
			this.timerId = setInterval(() => {
				if (this.isView('week')) {
					this.updateKey++;
				}
			}, 10 * 1000);
		},
		destroyed() {
			if (this.timerId)
				clearInterval(this.timerId);
		}
	});
})();
