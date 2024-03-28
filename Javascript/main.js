const fs = require("fs");
const { EigenvalueDecomposition } = require('ml-matrix');

function readCSV(filePath) {
  const csvData = fs.readFileSync(filePath, "utf-8");
  const rows = csvData.split("\n");
  const matrix = rows.map((row) => row.split(";"));
  return matrix;
}

function calculateStats(matrix) {
  const columnCount = matrix[0].length;
  const rowCount = matrix.length - 1; // Restar 1 si la primera fila contiene encabezados

  for (let j = 1; j < columnCount; j++) {
    // Asumiendo que la primera columna no contiene datos numéricos
    let columnSum = 0;
    let columnSumSquared = 0;
    let validValuesCount = 0;

    for (let i = 1; i < rowCount + 1; i++) {
      if (matrix[i][j] !== undefined) {
        // Verificar que el valor no sea undefined
        const valueStr = matrix[i][j].replace(",", "."); // Asegurarse de que el formato decimal sea correcto
        const value = parseFloat(valueStr);

        if (!isNaN(value)) {
          // Asegurarse de que el valor es un número
          columnSum += value;
          columnSumSquared += value * value;
          validValuesCount++;
        }
      }
    }

    const columnMean = columnSum / validValuesCount;
    const columnVariance =
      columnSumSquared / validValuesCount - columnMean ** 2;
    const columnStandardDeviation = Math.sqrt(columnVariance);

    for (let i = 1; i < rowCount + 1; i++) {
      if (matrix[i][j] !== undefined) {
        // Verificar nuevamente que el valor no sea undefined
        const valueStr = matrix[i][j].replace(",", ".");
        const value = parseFloat(valueStr);

        if (!isNaN(value)) {
          matrix[i][j] = (
            (value - columnMean) /
            columnStandardDeviation
          ).toFixed(2); // Formatear el resultado a 2 decimales
        } else {
          matrix[i][j] = "NaN"; // Manejar casos donde el parseo inicial falla
        }
      }
    }
  }
  return matrix;
}

function calculateCorrelationMatrix(matrix) {
  const columnCount = matrix[0].length;
  const rowCount = matrix.length - 1;

  const correlationMatrix = Array.from({ length: columnCount }, () =>
    Array(columnCount).fill(0)
  );

  for (let i = 0; i < columnCount; i++) {
    for (let j = 0; j < columnCount; j++) {
      if (i === j) {
        correlationMatrix[i][j] = 1; // Asigna 1 a los elementos de la diagonal
        continue; // Continúa al siguiente ciclo del bucle
      }
      if (i !== j) {
        let sumXY = 0;
        let sumX = 0;
        let sumY = 0;
        let sumXSquare = 0;
        let sumYSquare = 0;
        let validValuesCount = 0;

        for (let k = 0; k < rowCount + 1; k++) {
          if (matrix[k][i] !== undefined && matrix[k][j] !== undefined) {
            const valueStrX = matrix[k][i].replace(",", ".");
            const valueX = parseFloat(valueStrX);
            const valueStrY = matrix[k][j].replace(",", ".");
            const valueY = parseFloat(valueStrY);

            if (!isNaN(valueX) && !isNaN(valueY)) {
              sumXY += valueX * valueY;
              sumX += valueX;
              sumY += valueY;
              sumXSquare += valueX ** 2;
              sumYSquare += valueY ** 2;
              validValuesCount++;
            }
          }
        }

        const numerator = validValuesCount * sumXY - sumX * sumY;
        const denominator = Math.sqrt(
          (validValuesCount * sumXSquare - sumX ** 2) *
          (validValuesCount * sumYSquare - sumY ** 2)
        );
        const correlation = numerator / denominator;

        correlationMatrix[i][j] = correlation.toFixed(2);
      }
    }
  }

  return correlationMatrix;
}


//eliminar encabezados de una matriz
function removeHeaders(matrix) {
  //eliminar la primera columna y primera fila de una matriz
  let matrizSinPrimeraFila = matrix.slice(1);
  let matrizFinal = matrizSinPrimeraFila.map((fila) => fila.slice(1));
  return matrizFinal;
}

function printTable(matrix) {
  matrix.forEach((row) => {
    console.log(row.join("\t"));
  });
}

function printCorrelationMatrix(matrix) {
  matrix.forEach((row) => {
    console.log(row.join("\t"));
  });
}

function vectoresPropios(matrix) {
  let numericMatrix = matrix.map(row => row.map(Number));

  const resultado = new EigenvalueDecomposition(numericMatrix);
  return resultado;
}

function ordenarValoresPropios(valoresPropios, vectoresPropios) {
  let ordenados = valoresPropios
    .map((valor, i) => ({ valor, vector: vectoresPropios.map(row => row[i]) })) // Mapea cada valor propio a un objeto que contiene el valor y su vector propio correspondiente
    .sort((a, b) => b.valor - a.valor); // Ordena de mayor a menor según el valor propio

  // Extrae los valores y vectores propios ordenados
  const valoresOrdenados = ordenados.map(item => item.valor);
  const vectoresOrdenados = Array(valoresPropios.length).fill().map(() => Array(valoresPropios.length).fill(0));

  // Reorganiza los vectores propios ordenados en una matriz
  ordenados.forEach((item, i) => {
    item.vector.forEach((value, j) => {
      vectoresOrdenados[j][i] = value;
    });
  });

  return { valoresOrdenados, vectoresOrdenados };
}

function calcularMatrizComponentesPrincipales(matrix, vectoresPropiosOrdenados) {
  const matrizX = matrix.map(row => row.map(Number));
  const matrizV = vectoresPropiosOrdenados;

  const matrizComponentesPrincipales = matrizX.map(rowX => {
    return matrizV[0].map((colV, j) => {
      let sum = 0;
      rowX.forEach((valueX, k) => {
        sum += valueX * matrizV[k][j]; // Producto punto de la fila de X con la columna de V
      });
      return sum.toFixed(5);
    });
  });

  return matrizComponentesPrincipales;
}

//funcion producto punto de dos vectores
function productoPunto(vector1, vector2) {
  if (vector1.length !== vector2.length) {
    console.error('Los vectores deben tener la misma longitud');
    return NaN; // Manejo de error: longitud de vectores no coincide
  }
  let sum = 0;
  for (let i = 0; i < vector1.length; i++) {
    sum += vector1[i] * vector2[i];
  }
  return sum;
}

function matrizCalidadesIndividuales(matrizComponentesPrincipales, matrizNormal) {
  let matrizCalidadesIndividuales = [];

  for (let i = 0; i < matrizComponentesPrincipales.length; i++) {
    const row = [];
    for (let j = 0; j < matrizComponentesPrincipales[0].length; j++) {
      let numerador = Math.pow(matrizComponentesPrincipales[i][j], 2);
      let denominador = 0;

      for (let k = 0; k < matrizNormal[0].length; k++) {
        denominador += Math.pow(matrizNormal[i][k], 2);
      }

      // Verifica que el denominador no sea cero y que numerador y denominador no sean NaN
      if (denominador !== 0 && !isNaN(numerador) && !isNaN(denominador)) {
        row.push(numerador / denominador);
      }
    }
    matrizCalidadesIndividuales.push(row);
  }

  return matrizCalidadesIndividuales;
}

//column to array from a matrix and index
function columnToArray(matrix, index) {
  if (matrix.length === 0 || index < 0 || index >= matrix[0].length) {
    console.error('Índice de columna fuera de rango o matriz vacía');
    return []; // Manejo de error: índice fuera de rango o matriz vacía
  }
  let column = [];
  for (let i = 0; i < matrix.length; i++) {

    column.push(matrix[i][index]);
  }
  return column;
}

function matrizCoordenadasCalc(pca, vectoresPropiosCoordenadas) {
  const matrizResult = [];
  if (matrizComponentesPrincipales[0].length !== vectoresPropiosCoordenadas.length) {//validación
    console.error('Las dimensiones de las matrices no son compatibles para la multiplicación.');
    return null; 
  }
  for (let i = 0; i < pca.length; i++) {
    const row = [];
    for (let j = 0; j < pca[0].length; j++) {
      const vectorColumna = columnToArray(vectoresPropiosCoordenadas, j);
      const producto = productoPunto(matrizComponentesPrincipales[i], vectorColumna);
      if (producto !== 0) {
        row.push(producto);
      }
    }
    matrizResult.push(row);
  }
  return matrizResult;
}

function matrizCalidadesVarFun(vectoresOrdenados, matrizCoordenadas) {
  const matrizCalidadesVar = [];
  for (let i = 0; i < vectoresOrdenados.length; i++) {
    const row = [];
    for (let j = 0; j < vectoresOrdenados.length; j++) {
      let sum = 0;
      for (let k = 0; k < vectoresOrdenados[0].length; k++) {
        sum += matrizCoordenadas[i][k] * vectoresOrdenados[j][k];
      }
      row.push(sum);
    }
    matrizCalidadesVar.push(row);
  }
  return matrizCalidadesVar;
}

function varianza(valoresOrdenados) {
  let resultVar = [];
  let suma = valoresOrdenados[0];
  for (let i = 0; i < valoresOrdenados.length; i++) {
    suma += valoresOrdenados[i];
  }
  for (let i = 0; i < valoresOrdenados.length; i++) {
    resultVar.push((valoresOrdenados[i] / suma)*100);
  }
  return resultVar;
}

// Ejecución del programa

const filePath = "./EjemploEstudiantes.csv";
var matrix = readCSV(filePath);
matrix = calculateStats(matrix);
matrixNormalizada = removeHeaders(matrix);
console.log("Matriz de Datos Normalizados:");
printTable(matrix);

matrix = removeHeaders(matrix);

//PASO 2
console.log("Matriz de Correlaciones:");
const correlationMatrix = calculateCorrelationMatrix(matrix);
printCorrelationMatrix(correlationMatrix);

// PASO 3 Y 4: Usar la función ordenarValoresPropios para obtener valores y vectores propios ordenados
const result = vectoresPropios(correlationMatrix);
const valoresPropios = result.realEigenvalues;
const vectoresPropiosVar = result.eigenvectorMatrix.to2DArray();

const { valoresOrdenados, vectoresOrdenados } = ordenarValoresPropios(valoresPropios, vectoresPropiosVar);

console.log("\n3. Cálculo de valores propios ordenados");
console.log(valoresOrdenados);

console.log("\n4. Cálculo de vectores propios ordenados");
printTable(vectoresOrdenados);

// PASO 5: Calcular la matriz de componentes principales
const matrizComponentesPrincipales = calcularMatrizComponentesPrincipales(matrixNormalizada, vectoresOrdenados);

console.log("\nMatriz de Componentes Principales:");
printTable(matrizComponentesPrincipales);

//PASO 6 MATRIZ DE CALIDADES DE DE INDIVIDUOS
const matrizCalidadesIndividuales2 = matrizCalidadesIndividuales(matrizComponentesPrincipales, matrixNormalizada);
console.log("\nMatriz de Calidades de individuos:");

printTable(matrizCalidadesIndividuales2);

//paso 7 MATRIZ DE COORDENADAS
const matrizCoordenadasCorrelacion = calculateCorrelationMatrix(matrizComponentesPrincipales);
const vectoresPropiosCoordenadas = (vectoresPropios(matrizCoordenadasCorrelacion)).eigenvectorMatrix.to2DArray();

const matrizCoordenadas = matrizCoordenadasCalc(matrizComponentesPrincipales, vectoresPropiosCoordenadas);
console.log("\nMatriz de Coordenadas:");
printTable(matrizCoordenadas);

//PASO 8 matriz de calidades de las variables
const matrizCalidadesVar = matrizCalidadesVarFun(vectoresOrdenados, matrizCoordenadas);
console.log("\nMatriz de Calidades de Variables:");
printTable(matrizCalidadesVar);

//paso 9 Calcular el vector de inercias de los ejes 𝐼 ∈ 𝑀 1xm 
const varianzaVar = varianza(valoresOrdenados)
console.log("\nVector de Inercias:");
console.log(varianzaVar);
