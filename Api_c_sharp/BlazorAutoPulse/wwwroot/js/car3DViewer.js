// wwwroot/js/car3DViewer.js
window.car3DViewer = {
    scene: null,
    camera: null,
    renderer: null,
    controls: null,
    carModel: null,
    paintableMaterials: [],
    animationId: null,
    mixer: null,
    animations: [],
    currentAnimations: {},

    init: function(containerId, modelUrl) {
        console.log('Initialisation 3D...');

        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Container not found:', containerId);
            return false;
        }

        // Vérifier que THREE.js est chargé
        if (typeof THREE === 'undefined') {
            console.error('THREE.js n\'est pas chargé');
            return false;
        }

        // Nettoyer le container avant d'initialiser
        while (container.firstChild) {
            container.removeChild(container.firstChild);
        }

        // Scene
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0xf0f0f0);

        // Camera
        this.camera = new THREE.PerspectiveCamera(
            45,
            container.clientWidth / container.clientHeight,
            0.1,
            1000
        );
        this.camera.position.set(5, 2, 5);

        // Renderer
        this.renderer = new THREE.WebGLRenderer({
            antialias: true,
            alpha: true
        });
        this.renderer.setSize(container.clientWidth, container.clientHeight);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap; // Ombres plus douces
        this.renderer.outputEncoding = THREE.sRGBEncoding; // Meilleur rendu des couleurs
        this.renderer.toneMapping = THREE.ACESFilmicToneMapping; // Ton mapping réaliste
        this.renderer.toneMappingExposure = 1.0;
        container.appendChild(this.renderer.domElement);

        // Controls
        this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.05;
        this.controls.minDistance = 2;
        this.controls.maxDistance = 20;
        this.controls.maxPolarAngle = Math.PI / 2;

        // Lumières améliorées pour mieux voir les couleurs
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.8);
        this.scene.add(ambientLight);

        // Lumière principale (soleil)
        const directionalLight = new THREE.DirectionalLight(0xffffff, 1.2);
        directionalLight.position.set(10, 10, 5);
        directionalLight.castShadow = true;
        this.scene.add(directionalLight);

        // Lumière de remplissage (pour éclairer les zones sombres)
        const directionalLight2 = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight2.position.set(-10, 10, -5);
        this.scene.add(directionalLight2);

        // Lumière d'appoint sur le côté
        const directionalLight3 = new THREE.DirectionalLight(0xffffff, 0.6);
        directionalLight3.position.set(0, 5, -10);
        this.scene.add(directionalLight3);

        // Lumière du bas pour éviter les ombres trop sombres
        const hemisphereLight = new THREE.HemisphereLight(0xffffff, 0x444444, 0.6);
        this.scene.add(hemisphereLight);

        // Sol
        const groundGeometry = new THREE.PlaneGeometry(50, 50);
        const groundMaterial = new THREE.ShadowMaterial({ opacity: 0.3 });
        const ground = new THREE.Mesh(groundGeometry, groundMaterial);
        ground.rotation.x = -Math.PI / 2;
        ground.receiveShadow = true;
        this.scene.add(ground);

        // Charger le modèle
        this.loadModel(modelUrl);

        // Animation
        this.animate();

        // Gestion du redimensionnement
        this.resizeHandler = () => this.onWindowResize(container);
        window.addEventListener('resize', this.resizeHandler);

        return true;
    },

    loadModel: function(modelUrl) {
        if (typeof THREE.GLTFLoader === 'undefined') {
            console.error('GLTFLoader n\'est pas disponible !');
            return;
        }

        const loader = new THREE.GLTFLoader();

        loader.load(
            modelUrl,
            (gltf) => {
                console.log('Modèle chargé avec succès');
                this.carModel = gltf.scene;

                // Stocker les animations
                if (gltf.animations && gltf.animations.length > 0) {
                    console.log('Animations trouvées:', gltf.animations.length);
                    this.animations = gltf.animations;
                    this.mixer = new THREE.AnimationMixer(this.carModel);

                    // Logger les noms des animations
                    gltf.animations.forEach((anim, index) => {
                        console.log(`Animation ${index}: ${anim.name}`);
                    });
                } else {
                    console.log('Aucune animation trouvée dans le modèle');
                }

                // Centrer et dimensionner le modèle
                const box = new THREE.Box3().setFromObject(this.carModel);
                const center = box.getCenter(new THREE.Vector3());
                const size = box.getSize(new THREE.Vector3());

                const maxDim = Math.max(size.x, size.y, size.z);
                const scale = 3 / maxDim;
                this.carModel.scale.multiplyScalar(scale);

                this.carModel.position.sub(center.multiplyScalar(scale));
                this.carModel.position.y = 0;

                // Activer les ombres et trouver le matériau TwiXeR_W223
                this.carModel.traverse((node) => {
                    if (node.isMesh) {
                        node.castShadow = true;
                        node.receiveShadow = true;

                        // Recalculer les normales pour un rendu lisse
                        if (node.geometry) {
                            node.geometry.computeVertexNormals();
                        }

                        // Trouver le matériau TwiXeR_W223
                        if (node.material) {
                            if (Array.isArray(node.material)) {
                                node.material.forEach(mat => {
                                    // Activer le smooth shading
                                    mat.flatShading = false;

                                    // Détecter et configurer les matériaux de vitre
                                    if (this.isGlassMaterial(mat, node)) {
                                        this.configureGlassMaterial(mat);
                                    }

                                    if (mat.name === 'TwiXeR_W223') {
                                        this.paintableMaterials.push(mat);
                                    }
                                });
                            } else {
                                // Activer le smooth shading
                                node.material.flatShading = false;

                                // Détecter et configurer les matériaux de vitre
                                if (this.isGlassMaterial(node.material, node)) {
                                    this.configureGlassMaterial(node.material);
                                }

                                if (node.material.name === 'TwiXeR_W223') {
                                    this.paintableMaterials.push(node.material);
                                }
                            }
                        }
                    }
                });

                console.log('Matériaux TwiXeR_W223 trouvés:', this.paintableMaterials.length);

                // Appliquer les propriétés par défaut pour une meilleure visibilité
                this.paintableMaterials.forEach(mat => {
                    if (mat.metalness !== undefined) {
                        mat.metalness = 0.3;
                    }
                    if (mat.roughness !== undefined) {
                        mat.roughness = 0.7;
                    }
                });

                this.scene.add(this.carModel);
            },
            (xhr) => {
                const percent = (xhr.loaded / xhr.total * 100).toFixed(2);
                console.log(percent + '% chargé (' + xhr.loaded + ' / ' + xhr.total + ' octets)');
            },
            (error) => {
                console.error('Erreur de chargement du modèle:', error);
                console.error('URL du modèle:', modelUrl);
            }
        );
    },

    changeColor: function(hexColor) {
        if (this.paintableMaterials.length === 0) {
            console.warn('Aucun matériau TwiXeR_W223 trouvé');
            return;
        }

        const color = new THREE.Color(hexColor);
        this.paintableMaterials.forEach(material => {
            // Appliquer la couleur
            material.color.copy(color);

            // Ajuster les propriétés pour rendre la couleur plus visible
            if (material.metalness !== undefined) {
                material.metalness = 0.3; // Réduire l'aspect métallique
            }
            if (material.roughness !== undefined) {
                material.roughness = 0.7; // Augmenter la rugosité pour moins de reflets
            }

            // Forcer la mise à jour
            material.needsUpdate = true;
        });
        console.log('Couleur changée en:', hexColor);
    },

    playAnimation: function(animationName) {
        if (!this.mixer || this.animations.length === 0) {
            console.warn('Pas d\'animations disponibles');
            return false;
        }

        // Trouver l'animation par nom
        const animation = this.animations.find(anim => anim.name === animationName);

        if (!animation) {
            console.warn('Animation non trouvée:', animationName);
            return false;
        }

        // Arrêter l'animation précédente du même nom si elle existe
        if (this.currentAnimations[animationName]) {
            this.currentAnimations[animationName].stop();
        }

        // Jouer la nouvelle animation
        const action = this.mixer.clipAction(animation);
        action.reset();
        action.setLoop(THREE.LoopOnce);
        action.clampWhenFinished = true;
        action.play();

        this.currentAnimations[animationName] = action;
        console.log('Animation jouée:', animationName);
        return true;
    },

    reverseAnimation: function(animationName) {
        if (!this.mixer || this.animations.length === 0) {
            console.warn('Pas d\'animations disponibles');
            return false;
        }

        const animation = this.animations.find(anim => anim.name === animationName);

        if (!animation) {
            console.warn('Animation non trouvée:', animationName);
            return false;
        }

        if (this.currentAnimations[animationName]) {
            this.currentAnimations[animationName].stop();
        }

        const action = this.mixer.clipAction(animation);
        action.reset();
        action.setLoop(THREE.LoopOnce);
        action.clampWhenFinished = true;
        action.timeScale = -1; // Jouer à l'envers
        action.time = action.getClip().duration; // Commencer à la fin
        action.play();

        this.currentAnimations[animationName] = action;
        console.log('Animation inversée:', animationName);
        return true;
    },

    toggleAnimation: function(animationName) {
        if (!this.mixer || this.animations.length === 0) {
            return false;
        }

        const action = this.currentAnimations[animationName];

        // Si l'animation est en cours, on l'inverse
        if (action && action.isRunning()) {
            return this.reverseAnimation(animationName);
        } else {
            return this.playAnimation(animationName);
        }
    },

    getAnimationNames: function() {
        return this.animations.map(anim => anim.name);
    },

    isGlassMaterial: function(material, node) {
        if (!material) return false;

        const name = (material.name || '').toLowerCase();
        const nodeName = (node.name || '').toLowerCase();

        // Détecter si c'est un matériau de vitre par son nom
        const glassKeywords = ['glass', 'vitre', 'window', 'windshield',
            'pare-brise', 'fenetre', 'fenêtre', 'transparent'];

        return glassKeywords.some(keyword =>
            name.includes(keyword) || nodeName.includes(keyword)
        );
    },

    configureGlassMaterial: function(material) {
        // Activer la transparence
        material.transparent = true;
        material.opacity = 0.3; // Ajustez entre 0.1 (très transparent) et 0.5 (moins transparent)

        // Propriétés pour un verre réaliste
        if (material.metalness !== undefined) {
            material.metalness = 0.1;
        }
        if (material.roughness !== undefined) {
            material.roughness = 0.1; // Très lisse pour refléter
        }

        // Teinte légère pour le verre
        if (!material.color) {
            material.color = new THREE.Color(0x88ccff); // Bleu très clair
        } else {
            // Si le matériau est blanc, le remplacer par une teinte bleutée
            if (material.color.getHex() === 0xffffff) {
                material.color.setHex(0x88ccff);
            }
        }

        // Activer les propriétés de transmission si disponible (pour Three.js r128+)
        if (material.transmission !== undefined) {
            material.transmission = 0.9; // Transmission de lumière
        }

        // Double-sided pour voir à travers depuis l'intérieur
        material.side = THREE.DoubleSide;

        // Forcer la mise à jour
        material.needsUpdate = true;

        console.log('Matériau de vitre configuré:', material.name);
    },

    animate: function() {
        this.animationId = requestAnimationFrame(() => this.animate());

        // Mettre à jour les animations
        if (this.mixer) {
            this.mixer.update(0.016); // ~60fps
        }

        if (this.controls) {
            this.controls.update();
        }

        if (this.renderer && this.scene && this.camera) {
            this.renderer.render(this.scene, this.camera);
        }
    },

    onWindowResize: function(container) {
        if (!container || !this.camera || !this.renderer) return;

        this.camera.aspect = container.clientWidth / container.clientHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(container.clientWidth, container.clientHeight);
    },

    dispose: function() {
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
        }
        if (this.mixer) {
            this.mixer.stopAllAction();
        }
        if (this.renderer) {
            this.renderer.dispose();
            if (this.renderer.domElement && this.renderer.domElement.parentNode) {
                this.renderer.domElement.parentNode.removeChild(this.renderer.domElement);
            }
        }
        if (this.controls) {
            this.controls.dispose();
        }
        if (this.resizeHandler) {
            window.removeEventListener('resize', this.resizeHandler);
        }
        this.paintableMaterials = [];
        this.carModel = null;
        this.animations = [];
        this.currentAnimations = {};
        this.mixer = null;
    }
};