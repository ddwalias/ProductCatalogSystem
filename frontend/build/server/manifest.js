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
		client: {start:"_app/immutable/entry/start.Cik-PV6t.js",app:"_app/immutable/entry/app.BoZ9GgWP.js",imports:["_app/immutable/entry/start.Cik-PV6t.js","_app/immutable/chunks/C-WQttD9.js","_app/immutable/chunks/CbZZf24z.js","_app/immutable/chunks/B8R26iTA.js","_app/immutable/entry/app.BoZ9GgWP.js","_app/immutable/chunks/B8R26iTA.js","_app/immutable/chunks/DsnmJJEf.js","_app/immutable/chunks/CbZZf24z.js","_app/immutable/chunks/BwexrRJu.js"],stylesheets:[],fonts:[],uses_env_dynamic_public:false},
		nodes: [
			__memo(() => import('./chunks/0-ASmERItv.js')),
			__memo(() => import('./chunks/1-DVW6h_Ma.js')),
			__memo(() => import('./chunks/2-B0rsl_9X.js')),
			__memo(() => import('./chunks/3-CBqg_8LB.js')),
			__memo(() => import('./chunks/4-xiZ5HHj4.js'))
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
