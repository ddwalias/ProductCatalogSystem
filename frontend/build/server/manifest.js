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
		client: {start:"_app/immutable/entry/start.DAryB3UB.js",app:"_app/immutable/entry/app.Dq7rIkrd.js",imports:["_app/immutable/entry/start.DAryB3UB.js","_app/immutable/chunks/CYpSNn-a.js","_app/immutable/chunks/gxjHH5eP.js","_app/immutable/entry/app.Dq7rIkrd.js","_app/immutable/chunks/gxjHH5eP.js","_app/immutable/chunks/CsaAgWqW.js","_app/immutable/chunks/Ci8oCWGg.js","_app/immutable/chunks/BsksWOdh.js","_app/immutable/chunks/LWOE2mpq.js"],stylesheets:[],fonts:[],uses_env_dynamic_public:false},
		nodes: [
			__memo(() => import('./chunks/0-DNANydIk.js')),
			__memo(() => import('./chunks/1-ClVo7Ylq.js')),
			__memo(() => import('./chunks/2-DyL7Dgdi.js')),
			__memo(() => import('./chunks/3-q18rC-57.js')),
			__memo(() => import('./chunks/4-BNJTjlKK.js'))
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
