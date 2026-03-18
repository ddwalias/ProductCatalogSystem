const manifest = (() => {
function __memo(fn) {
	let value;
	return () => value ??= (value = fn());
}

return {
	appDir: "_app",
	appPath: "_app",
	assets: new Set([]),
	mimeTypes: {},
	_: {
		client: {start:"_app/immutable/entry/start.DVrxlauM.js",app:"_app/immutable/entry/app.CZGgZDVw.js",imports:["_app/immutable/entry/start.DVrxlauM.js","_app/immutable/chunks/Bl2wKSbc.js","_app/immutable/chunks/BygCpMsz.js","_app/immutable/chunks/Bq4W124j.js","_app/immutable/entry/app.CZGgZDVw.js","_app/immutable/chunks/Bq4W124j.js","_app/immutable/chunks/DsnmJJEf.js","_app/immutable/chunks/BygCpMsz.js","_app/immutable/chunks/BOgxV9Ht.js"],stylesheets:[],fonts:[],uses_env_dynamic_public:false},
		nodes: [
			__memo(() => import('./chunks/0-1jsOMtXS.js')),
			__memo(() => import('./chunks/1-7-ugFyZd.js')),
			__memo(() => import('./chunks/2-CD04g4Sk.js')),
			__memo(() => import('./chunks/3-C1s-P0sv.js')),
			__memo(() => import('./chunks/4-BEta2aHN.js'))
		],
		remotes: {
			
		},
		routes: [
			{
				id: "/",
				pattern: /^\/$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 2 },
				endpoint: null
			},
			{
				id: "/categories",
				pattern: /^\/categories\/?$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 3 },
				endpoint: null
			},
			{
				id: "/products",
				pattern: /^\/products\/?$/,
				params: [],
				page: { layouts: [0,], errors: [1,], leaf: 4 },
				endpoint: null
			}
		],
		prerendered_routes: new Set([]),
		matchers: async () => {
			
			return {  };
		},
		server_assets: {}
	}
}
})();

const prerendered = new Set([]);

const base = "";

export { base, manifest, prerendered };
//# sourceMappingURL=manifest.js.map
