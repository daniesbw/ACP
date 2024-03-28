import numpy as np
import matplotlib.pyplot as plt


componentes_principales = np.array([
  [-0.32346, -1.77402, 1.19899, 0.03577, -0.00750],
  [-0.66676, 1.64559, 0.14898, 0.02144, 0.12611],
  [-0.99953, 0.51691, 0.62724, -0.51498, -0.15434],
  [3.16884, 0.26524, -0.39482, -0.67011, 0.05515],
  [0.48737, -1.36450, -0.84151, 0.16703, -0.11604],
  [-1.70532, 1.02821, -0.12027, -0.06770, -0.02515],
  [-0.06852, -1.45789, -0.50469, 0.12643, -0.01278],
  [-2.00936, 1.27711, -0.53059, 0.20649, -0.00904],
  [3.04643, 1.25306, 0.44830, 0.63951, -0.02143],
  [-0.92871, -1.36612, -0.02499, 0.06762, 0.18414]
])

nombres_estudiantes = ["Lucia", "Pedro", "Ines", "Luis", "Andres", "Ana", "Carlos", "Jose", "Sonia", "Maria"]


x = componentes_principales[:, 0]
y = componentes_principales[:, 1]


fig, ax = plt.subplots(figsize=(10, 10))
ax.scatter(x, y, color='blue')


for i, nombre in enumerate(nombres_estudiantes):
    ax.text(x[i], y[i], nombre, fontsize=9)


ax.set_title("Gráfico en el Plano Principal")
ax.set_xlabel("Componente Principal 1")
ax.set_ylabel("Componente Principal 2")
ax.grid(True)

# Mostrar el gráfico
plt.show()
